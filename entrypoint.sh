#!/bin/bash
set -e

mkdir -p /data/rulesets

for f in /tmp/rulesets/*.dll; do
    if [ -f "$f" ]; then
        dest="/data/rulesets/$(basename "$f")"
        if [ ! -f "$dest" ]; then
            echo "Copying $(basename "$f") to /data/rulesets"
            cp "$f" "$dest"
        else
            echo "$(basename "$f") already exists, skipping"
        fi
    fi
done

exec dotnet /app/PerformanceServer.dll
