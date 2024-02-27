# School project for Microservices and containers

## Description
This project is a collection of microservices running in docker containers for
managing traffic lights. This includes two sub-tasks for the course: the deployment
of microservices which is under branch exercise/deployment and the main course project
which can be found under master branch.

### Components

* Control unit manages the logic for using traffic lights
* Traffic light service mimics the traffic lights and acts under control unit's commands
* RabbitMQ messaging is used for queues between control unit and traffic lights.

### Branching
* exercise/deployment for task "Deployment of microservices"
* master for task "Course project"

## Setup

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.
- [Docker](https://www.docker.com/) installed on your machine.

### Running the Application

1. Clone the repository to local machine.
2. Run with docker compose up.
    ```
    docker-compose up
    ```
3. The ControlUnit will now start managing the traffic lights, and the TrafficLight project will simulate the behavior of traffic lights.

## Configuration

- RabbitMQ host and credentials can be configured using environment variables. By default, the application will attempt to connect to RabbitMQ running on `localhost` with the default credentials (`guest:guest`).
- You can access RabbitMQ UI at http://localhost:15672/ when docker container is running.
