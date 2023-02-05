# What

[Spaced repetition](https://en.wikipedia.org/wiki/Spaced_repetition) telegram bot to help you recall info and remember all your stuff literally FOREVER*. 

# How

1. [Install](https://gist.github.com/B1Z0N/0d03a2b6dfb7fdca3efeaa9802f443d8) postgresql
2. Run `./scripts/setup.sh` or
    - Create user `nevermindy` and database `nevermindydb` and grant all privileges on it to that user.
    - Fulfill `appsettings.exmpl.json` with your data and rename it to `appsettings.prod.json`.
3. Run `./scripts/run.sh` or 
    - Open `src/nevermindy.csproj` in your IDE and run it.

# Why

Because It is very effective way of learning new things. 

Personally I use it this way: 

1. Doing something for some time(few days-few months). Like a deep dive into smth.
2. Writing summary or thesis of what I've learned from this "deep" learning experience.
3. Posting it to this bot so that I could get this stuff nested into my lifestyle.
4. Profit.

# Todo

1. Docker
2. Localization to a lot of languages.
3. Spam protection.

For others possible addons, see open issues.

# PS

*for your lifetime
