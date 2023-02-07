#!/bin/bash

script_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$script_path"

PSQL_HOST="0.0.0.0"
PSQL_PORT="5432"

echo    "[!nevermindy] Connecting to your database, please specify a host(default=$PSQL_HOST)"
echo -n "[nevermindy] Enter the host(press enter for default): "
read -r host
host=${host:-$PSQL_HOST}

echo    "[nevermindy] Connecting to your database, please specify a port(default=\"5432\")"
echo -n "[nevermindy] Enter the port(press enter for default): "
read -r port
port=${port:-$PSQL_PORT}

psql -h $host -p $port -U postgres -f  drop.sql
