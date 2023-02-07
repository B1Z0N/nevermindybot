#!/usr/bin/env bash

script_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$script_path"

FIBONACCI_TIMESPAN_FIRST="1.00:00:00"
FIBONACCI_TIMESPAN_SECOND="3.00:00:00"
POLLING_INTERVAL_TIMESPAN="00:20:00"
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

echo    "[!nevermindy] Creating the database nevermindydb for user nevermindy."

echo -n "[nevermindy] Please enter a password: "
read -r password

echo -n "[!nevermindy] Please enter bot token(from BotFather): "
read -r token

echo    "[!nevermindy] Choosing timespan(TimeSpan.ToString()) for first fibonacci element(default=$FIBONACCI_TIMESPAN_FIRST)."
echo -n "[nevermindy] Enter the timespan(press enter for default): "

read -r first_fib_timespan
first_fib_timespan=${first_fib_timespan:-$FIBONACCI_TIMESPAN_FIRST}

echo    "[nevermindy] Choosing timespan(TimeSpan.ToString()) for second fibonacci element(default=$FIBONACCI_TIMESPAN_SECOND)."
echo -n "[nevermindy] Enter the timespan(press enter for default): "

read -r second_fib_timespan
second_fib_timespan=${second_fib_timespan:-$FIBONACCI_TIMESPAN_SECOND}

echo    "[nevermindy] Choosing timespan(try TimeSpan.ToString()) for background job polling interval(default=$POLLING_INTERVAL_TIMESPAN)."
echo -n "[nevermindy] Enter the timespan(press enter for default): "

read -r polling_interval_timespan
polling_interval_timespan=${polling_interval_timespan:-$POLLING_INTERVAL_TIMESPAN}

psql -h $host -p $port -U postgres -f setup_db.sql -v password="'${password}'"
psql -h $host -p $port -U postgres -d nevermindydb -f setup_schema.sql 
connection_string="User ID=nevermindy;Password=${password};Host=localhost;Port=5432;Database=nevermindydb;"

res=$(sed -e "s/YOUR_BOT_ACCESS_TOKEN/$token/g" "../appsettings.exmpl.json" \
| sed -e "s/YOUR_CONNECTION_STRING/$connection_string/g" \
| sed -e "s/YOUR_FIBONACCI_TIMESPAN_FIRST/$first_fib_timespan/g" \
| sed -e "s/YOUR_FIBONACCI_TIMESPAN_SECOND/$second_fib_timespan/g" \
| sed -e "s/YOUR_POLLING_INTERVAL_TIMESPAN/$polling_interval_timespan/g")

echo "$res" > "../appsettings.prod.json"
