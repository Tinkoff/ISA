FROM docker.elastic.co/elasticsearch/elasticsearch:6.6.0 AS elastic

# Copy files for indexes
WORKDIR ./config/hunspell/en_US
COPY ./additionalConfig/elasticsearch/hunspell/en_US ./

WORKDIR ../ru_RU
COPY ./additionalConfig/elasticsearch/hunspell/ru_RU ./

WORKDIR ../../solrAcronyms/en_US
COPY ./additionalConfig/elasticsearch/solrAcronyms/en_US ./

WORKDIR ../../solrSynonyms/en_US
COPY ./additionalConfig/elasticsearch/solrSynonyms/en_US ./

WORKDIR ../ru_RU
COPY ./additionalConfig/elasticsearch/solrSynonyms/ru_RU ./

WORKDIR ../../solrSynonymsAcronyms/en_US
COPY ./additionalConfig/elasticsearch/solrSynonymsAcronyms/en_US ./

# Add scripts and indexes configuration json files in order to create indexes after raising es
WORKDIR ../../../utils
COPY ./utils/wait-for-it.sh ./
COPY ./additionalConfig/elasticsearch/create-indexes.sh ./
COPY ./additionalConfig/elasticsearch/*.json ./


# Launch es and wait until it is up and create indexes
WORKDIR ..
RUN su -c "./bin/elasticsearch -d -p espid.tmp" elasticsearch; \
    /bin/bash ./utils/wait-for-it.sh -t 0 localhost:9200 -- ./utils/create-indexes.sh; \
    kill $(cat espid.tmp) && \
    exit 0;