pushd ../..

docker image build -f ./build/elasticsearch.dockerfile . -t tinkoff-isa-elasticsearch:0.1

docker run -d --name tinkoff-isa-elasticsearch -p 9200:9200 tinkoff-isa-elasticsearch:0.1
docker run -d --name tinkoff-isa-mongo -p 27017:27017 mongo:3.6

popd