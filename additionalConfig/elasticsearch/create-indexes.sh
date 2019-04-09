#!/bin/bash
base="http://localhost:9200"

echo $(curl -X PUT $base/questions -d "@./utils/questionsIndex.json" -H "Content-Type: application/json")
echo $(curl -X PUT $base/answers -d "@./utils/answersIndex.json" -H "Content-Type: application/json")
echo $(curl -X PUT $base/confluence -d "@./utils/confluenceIndex.json" -H "Content-Type: application/json")
echo $(curl -X PUT $base/jira -d "@./utils/jiraIndex.json" -H "Content-Type: application/json")