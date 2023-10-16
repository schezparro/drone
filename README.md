# DRONE Application
This is a test app for managing drones and medications.

## Key Features:
To run this application, you will need to have the .NET SDK 7.0 or later installed.
You can download it from: [https://dotnet.microsoft.com/en-us/download/dotnet/7.0.]

## Installation
1. Open the terminal and run:
    ```
    dotnet restore
    dotnet run
    ```
2. Enjoy!!!

## Usage:
1. Navigate to [http://localhost:5286/swagger/index.html] to access the Swagger documentation. This will allow you to use the available endpoints for:

   - Checking available drones for loading.
   - Registering a drone.
   - Loading a drone with medication items.
   - Checking loaded medication items for a given drone.
   - Checking drone battery level for a given drone.

2. Go to [http://localhost:5286/hangfire] to manage the recurrent job, which includes:

   - One periodic task to check drone battery levels and create a history/audit event log for this.

3. Additionally, you can run the tests using:
    ```
    dotnet test
    ```
   This simulates the Recurrent Job for events and generates a file "events.txt" in the same folder as the project, with the events registered when the drone battery level changes.

## Contact:
For further inquiries, please contact schezparro@gmail.com.