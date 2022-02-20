#!/usr/bin/env bash

script_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$script_path"

echo "[nevermindy] Creating the database nevermindydb for user nevermindy."
echo -n "[nevermindy] Please enter a password: "
read password
echo -n "[nevermindy] Please enter bot token(from BotFather): "
read token

psql postgres -f setup.sql -v password="'${password}'"

connection_string="User ID=nevermindy;Password=${password};Host=localhost;Port=5432;Database=nevermindydb;"
echo "{\"ACCESS_TOKEN\":\"${token}\",\"CONNECTION_STRING\":\"${connection_string}\"}" > appsettings.prod.json
