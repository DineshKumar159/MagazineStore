#Magazine Store (List of Magazines)

##Purpose
To identify all subscribers that are subscribed to atleast one magazine in each category.

Prerequisites
•	.NET Core SDK installed on your machine.
Getting Started
1.	Unzip the zip file ListOfMagazines.zip.
2.	Open the solution file (ListOfMagazines.sln) in Visual Studio or your preferred IDE. 
3.	Build the solution to restore packages and compile the code.
Running the Application
1.	Set the startup project to the console application project (ListOfMagazines).
2.	Press F5 or use the "Start" button in your IDE to run the program.
3.	Alternatively, you can navigate to the following path: 'ListOfMagazines\ListOfMagazines\bin\Debug\net7.0' and double-click on the 'ListOfMagazines.exe' file to launch the application.
Performance
As per the guidelines in the problem document, the solution presented here has successfully met the performance benchmark of less than 10 seconds. This was accomplished by executing independent tasks (each retrieving magazine details for a specific category) as parallel asynchronous operations.
