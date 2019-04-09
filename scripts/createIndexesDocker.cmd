@echo off

echo. "Script last updated date 14.08.2018"
echo. "Creating of elastic indexes: questions and answers"

set a=http://localhost:9200

curl -X PUT %a%/questions -d "@.\..\AdditionalConfig\elasticsearch\questionsIndex.json" -H "Content-Type: application/json"
echo. &
curl -X PUT %a%/answers -d "@.\..\AdditionalConfig\elasticsearch\answersIndex.json" -H "Content-Type: application/json"
echo. &
curl -X PUT %a%/confluence -d "@.\..\AdditionalConfig\elasticsearch\confluenceIndex.json" -H "Content-Type: application/json"
echo. &
curl -X PUT %a%/jira -d "@.\..\AdditionalConfig\elasticsearch\jiraIndex.json" -H "Content-Type: application/json"

:Done
Echo. Done
pause
exit /B
