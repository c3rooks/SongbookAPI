# Guitar Songbook API

This project is a .NET Core Web API that collects and scrapes guitar tabs from various sources, including Ultimate Guitar. The end goal of this project is to integrate with Spotify accounts and playlists, allowing users to easily access guitar tabs related to their favorite music. This will make it easier to learn songs and have a catalog of tabs right at your fingertips, with audio playback available for each song.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

What things you need to install the software and how to install them:

- .NET Core 3.1 or later
- MongoDB server

### Installing

A step-by-step guide to get the development environment up and running.

1. Clone the repository:

   ```bash
   git clone https://github.com/yourusername/guitar-songbook-api.git
   cd guitar-songbook-api
2. Restore the dependencies: dotnet restore
3. Configure your MongoDB connection string in `appsettings.json`.

### Running the API
To run the API, use the following command: dotnet run
Then, navigate to https://localhost:5001/ to interact with the API.

### Endpoints

This API includes the following endpoints:

- `GET /songs`: Get all songs.
- `GET /songs/{id}`: Get a song by ID.
- `POST /songs`: Create a new song.
- `PUT /songs/{id}`: Update an existing song by ID.
- `DELETE /songs/{id}`: Delete a song by ID.
- `GET /songs/tab/{id}`: Get tab information for a song.
- `GET /songs/tabs/{songName}/{artistName}`: Get tab information for a song by its name and artist.

### Contributing
If you wish to contribute to this project, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository.


Please update the placeholders with the correct values specific to your project, such as the repository URL and MongoDB connection string. Let me know if you need any further assistance!
