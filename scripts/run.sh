#!/usr/bin/env bash

script_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$script_path"

until dotnet run --project ../src/nevermindy.csproj 2>&1 | tee -a ../log.txt; do
    echo "Nevermindy crashed with exit code $?.  Respawning.." >&2
    sleep 1
done
