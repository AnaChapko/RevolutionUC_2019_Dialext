WELCOME and thanks for checking this project out! This is Dialext - an SMS-based group chat which seamlessly translates all incoming messages to each users' preferred language! It was a RevolutionUC_2019 Hackathon submission (more info here: https://devpost.com/software/dialext ) and won 3 awards!

This project was run using Visual Studio, ngrok, Twilio SMS Messaging API, and Microsoft Azure Translator Text API. This project is a work in progress, so please bear with the semi-sloppily put together mess that it currently is.

Currently I've uploaded both Visual Studio projects and an example of the text files used as temporary text-based databases by the program to this git repo. For Dialext to function correctly in its currentn state, you need to run both visual studio projects simultaneously. The "Receive Response" project is called automatically through Twilio whenever the Twilio number receives a message. The "Version 1" project is always running and handles the input received/stored by the "Received Response" project. In other words, the "Version 1" project is most of the code that actually does stuff. 

I've also just copied the two C# source code files to the forefront so you can skim through that if you'd like.

(Note: Make sure to change the sensitive info (phone numbers/authentication keys/etc) from the current gibberish values to valid values before attempting to run.)

-Anastasiya
