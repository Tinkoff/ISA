@echo off
if "%~1"=="" (Call :parametersCheck& Exit /B)

echo. "Script last updated date 14.08.2018"

call checkElastic.bat %~1

::elastic
xcopy ".\..\AdditionalConfig\elasticsearch\*.*" "%~1" /e
echo. "Creating of elastic indexes: questions and answers"

if "%~2"=="" (
	set a=http://localhost:9200) else (
	set a=%~2)


curl -X PUT %a%/questions -d "@.\..\AdditionalConfig\elasticsearch\questionsIndex.json" -H "Content-Type: application/json"
echo. &
curl -X PUT %a%/answers -d "@.\..\AdditionalConfig\elasticsearch\answersIndex.json" -H "Content-Type: application/json"
echo. &
curl -X PUT %a%/confluence -d "@.\..\AdditionalConfig\elasticsearch\confluenceIndex.json" -H "Content-Type: application/json"
echo.&
curl -X PUT %a%/jira -d "@.\..\AdditionalConfig\elasticsearch\jiraIndex.json" -H "Content-Type: application/json"

goto Done

:parametersCheck
Echo. Wrong input
Echo. Please, enter parameters, like:
Echo. %~nx0 [Elastic config Directory] [opt. Elastic Url]
Echo.
Pause
exit /B 

:Done
Echo. Done
pause
exit /B
