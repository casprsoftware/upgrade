# SQL Upgrade Tool

Little tool for running sql scripts from directory.

## Build

For build the tool run powershell script **build.ps1**. In `dist` directory you will find exe file **UpgradeConsole.exe**.

## Run

Run exe file without parameters. It will show you possible parameters and their descriptions.

### Examples

```console

UpgradeConsole.exe "connection-string to sql db" --directory "./myscripts"

```
