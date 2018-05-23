
docker tag publisher akstutorialacr.azurecr.io/publisher
docker tag consumer akstutorialacr.azurecr.io/consumer

docker push akstutorialacr.azurecr.io/publisher
docker push akstutorialacr.azurecr.io/consumer