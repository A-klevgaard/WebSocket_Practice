# WebSocket_Practice
A simple solution to explore using websockets to connect a client and server program

The client side connects to the server address, and then uses a trackbar to select a number between 1 and 1000 to send to the database

The server side randomly chooses a number between 1 - 1000 inclusive as the "secret number" and returns a response to the client
if the number they guess is too high, too low, or correct. If a correct guess is sent then the server automatically chooses 
a new secret number and the game begins again. 

Each wrong secret guess will adjust the limits of the numerical trackbar to helpthe client hone in on the correct secret number.
