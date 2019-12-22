pushd ../..

compose_file_path="./build/docker-compose.yml"

docker image build -f ./build/base.build.dockerfile . -t tinkoff-isa-build-base:0.1

docker-compose -f $compose_file_path kill
docker-compose -f $compose_file_path rm -f
docker-compose -f $compose_file_path build
docker-compose -f $compose_file_path up -d

popd