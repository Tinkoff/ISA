@echo off

if not exist %~1 goto checkElasticFolder

if not exist ".\..\AdditionalConfig\ElasticSearch\solrSynonyms\ru_RU\synonyms.txt" goto checkRuSynonyms
if not exist ".\..\AdditionalConfig\ElasticSearch\solrSynonymsAcronyms\en_US\synonymsAcronyms.txt" goto checkEnSynonymsAcronyms

if not exist ".\..\AdditionalConfig\ElasticSearch\hunspell\ru_RU\ru_RU.aff" goto checkHunspell
if not exist ".\..\AdditionalConfig\ElasticSearch\hunspell\ru_RU\ru_RU.dic" goto checkHunspell
if not exist ".\..\AdditionalConfig\ElasticSearch\hunspell\en_US\en_US.aff" goto checkHunspell
if not exist ".\..\AdditionalConfig\ElasticSearch\hunspell\en_US\en_US.dic" goto checkHunspell

exit /B


:checkElasticFolder
echo. Error. Incorrect path to the elastic folder 
pause
call :__stop

:checkRuSynonyms
echo. Error. Incorrect path to the Russian synonyms file. Path should be:
echo. ".\..\AdditionalConfig\ElasticSearch\solrSynonyms\ru_RU\synonyms.txt"
pause
call :__stop

:checkEnSynonymsAcronyms
echo. Error. Incorrect path to the English synonyms and acronyms file. Path should be:
echo. ".\..\AdditionalConfig\ElasticSearch\solrSynonymsAcronyms\en_US\synonymsAcronyms.txt"
pause
call :__stop

:checkHunspell
echo. Error. Incorrect path to the hunspell filter files. Paths should be : 
echo. .\..\AdditionalConfig\ElasticSearch\hunspell\ru_RU\ru_RU.aff
echo. .\..\AdditionalConfig\ElasticSearch\hunspell\ru_RU\ru_RU.txt
echo. .\..\AdditionalConfig\ElasticSearch\hunspell\en_US\en_US.aff
echo. .\..\AdditionalConfig\ElasticSearch\hunspell\en_US\en_US.txt
pause
call :__stop 

:__stop
()