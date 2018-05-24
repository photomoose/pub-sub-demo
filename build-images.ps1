docker build -f ./Publisher/Dockerfile -t publisher ./Publisher
docker build -f ./Consumer/Dockerfile -t consumer ./Consumer
docker build -f ./Web/Dockerfile -t web ./Web
