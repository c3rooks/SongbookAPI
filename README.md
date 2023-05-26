# Guitar Songbook API

This project is a .NET Core Web API that interacts with a MongoDB database to store and manage songs with guitar chords. This API serves as a backend for a Guitar Songbook application.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

What things you need to install the software and how to install them:

- .NET Core 3.1 or later
- MongoDB server

### Installing

A step by step series of examples that tell you how to get a development environment running.

1. Clone the repository:

   ```bash
   git clone https://github.com/yourusername/guitar-songbook-api.git
   cd guitar-songbook-api
2. Restore the dependencies:
   dotnet restore
3. Configure your MongoDB connection string in `appsettings.json`.

### Running the API

To run the API, use the `dotnet run` command

Then, navigate to `https://localhost:5001/` to interact with the API.

## Endpoints

This API includes the following endpoints:

- `GET /songs`: Get all songs.
- `GET /songs/{id}`: Get a song by ID.
- `POST /songs`: Create a new song.
- `PUT /songs/{id}`: Update an existing song by ID.
- `DELETE /songs/{id}`: Delete a song by ID.

## Contributing

If you wish to contribute to this project, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository.


