import torch
import cv2
import numpy as np
from mss import mss
import socket
import time

# Load the model with your custom weights
model = torch.hub.load('ultralytics/yolov5', 'custom', path='C:/Users/Jared/Desktop/Code/AI_Poker/bestModel.pt')

# Define the class names
class_names = ['10c', '10d', '10h', '10s', '2c', '2d', '2h', '2s', '3c', '3d', '3h', '3s',
               '4c', '4d', '4h', '4s', '5c', '5d', '5h', '5s', '6c', '6d', '6h', '6s',
               '7c', '7d', '7h', '7s', '8c', '8d', '8h', '8s', '9c', '9d', '9h', '9s',
               'Ac', 'Ad', 'Ah', 'As', 'Jc', 'Jd', 'Jh', 'Js', 'Kc', 'Kd', 'Kh', 'Ks', 
               'Qc', 'Qd', 'Qh', 'Qs']

# TCP Server details
SERVER_IP = '127.0.0.1'  # Replace with your server's IP if it's on another machine
SERVER_PORT = 8052        # Must match the port in your Unity server

# Set to keep track of seen class indices
seen_class_indices = set()
last_detected_indices = []

# TCP client for maintaining a persistent connection
client_socket = None

def start_tcp_connection():
    global client_socket
    try:
        # Create a TCP/IP socket and connect to the server
        client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client_socket.connect((SERVER_IP, SERVER_PORT))
        print(f"Connected to server at {SERVER_IP}:{SERVER_PORT}")
    except Exception as e:
        print(f"Failed to connect to server: {e}")

def send_message_to_server(message):
    try:
        # Send a message to the server
        client_socket.sendall(message.encode('utf-8'))
        print(f"Sent: {message}")
    except Exception as e:
        print(f"Failed to send message to server: {e}")

# Function to run inference on the live desktop screen
def run_inference_on_screen():
    global client_socket, last_detected_indices
    # Define the screen capture area (you can adjust the 'mon' dictionary)
    mon = {'top': 0, 'left': 0, 'width': 1920, 'height': 1080}
    sct = mss()
    
    # Start the TCP connection
    start_tcp_connection()
    
    while True:
        # Capture the screen
        screen = np.array(sct.grab(mon))
        frame = cv2.cvtColor(screen, cv2.COLOR_BGRA2BGR)  # Convert from BGRA to BGR
        
        # Run inference
        results = model(frame)
        
        # Get the detected class IDs and confidence scores
        predictions = results.pred[0]
        class_ids = predictions[:, -1].cpu().numpy()  # Move tensor to CPU and then convert to numpy
        confidences = predictions[:, 4].cpu().numpy()  # Get confidence scores
        
        # Filter classes with confidence above 45%
        for i, confidence in enumerate(confidences):
            if confidence > 0.45:  # Only consider detections with > 45% confidence
                detected_index = int(class_ids[i])
                if detected_index not in seen_class_indices:
                    seen_class_indices.add(detected_index)
                    last_detected_indices.append(detected_index)  # Add to last detected indices
        
        # Print the detected class indices
        print(f"Detected class indices: {seen_class_indices}")
        print(f"Total unique classes seen: {len(seen_class_indices)}")
        
        # Send the current count of unique classes to the TCP server
        if len(seen_class_indices) <= 41:
            send_message_to_server(f"Cards Seen: {len(seen_class_indices)}")
        else:
            # When we have more than 41 unique classes detected, send only the last detected indices
            last_indices_message = f"{last_detected_indices[-11:]}"  # Sending the last 10 detected indices
            send_message_to_server(last_indices_message)
            print(f"Final message sent: {last_indices_message}")
            break  # Exit the loop after sending the message
        
        # Draw the bounding boxes and labels on the frame
        results.render()  # Updates the frame with bounding boxes and labels
        
        # Display the frame
        cv2.imshow('Screen Capture', frame)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
    
    # Release resources
    cv2.destroyAllWindows()

    # Close the TCP connection once we're done
    if client_socket:
        client_socket.close()
        print("Connection closed.")

# Example usage
run_inference_on_screen()
