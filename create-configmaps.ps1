kubectl create configmap publisher-appsettings --from-file=./Config/Publisher/appsettings.production.json
kubectl create configmap consumer-appsettings --from-file=./Config/Consumer/appsettings.production.json