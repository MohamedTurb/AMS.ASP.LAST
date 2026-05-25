#!/usr/bin/env bash
set -euo pipefail

echo "Starting AMS in production using docker-compose"

if [ ! -f .env ]; then
  echo ".env file not found. Copy .env.example to .env and fill values." >&2
  exit 1
fi

docker compose -f docker-compose.yml pull || true
docker compose -f docker-compose.yml up --build -d

echo "Waiting for health endpoint..."
for i in {1..20}; do
  if curl -fsS http://localhost:5002/health >/dev/null 2>&1; then
    echo "Service is healthy"
    exit 0
  fi
  sleep 3
done

echo "Service did not become healthy in time." >&2
exit 2
