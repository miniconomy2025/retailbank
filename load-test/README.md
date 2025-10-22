# Locust Load Testing

A basic Locust application for API load testing.

## Installation

Install the required dependencies:

```bash
pip install -r requirements.txt
```

## Usage

### Running with Web UI

Start Locust with the web interface:

```bash
locust --host=https://url-goes-here.com
```

Then open your browser to http://localhost:8089 and configure:
- Number of users to simulate
- Spawn rate (users started per second)

Parameters:
- `--host`: Target API endpoint
- `--users`: Total number of concurrent users
- `--spawn-rate`: Users to spawn per second
- `--run-time`: Duration of the test (e.g., 60s, 5m, 1h)
- `--headless`: Run without web UI