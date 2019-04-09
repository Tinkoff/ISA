@echo off
FOR /f "tokens=*" %%i IN ('docker ps -aq') DO docker rm -f %%i
FOR /f "tokens=*" %%i IN ('docker images --format "{{.ID}}"') DO docker rmi -f %%i