#pobranie obrazu .net sdk 5.0 
FROM mcr.microsoft.com/dotnet/sdk:8.0
#skopiowanie plików i folderów z obecnej œcie¿ki do folderu /app w powstaj¹cym obrazie
COPY . /app
#ustawienie folderu /app w powstaj¹cym obrazie jako œcie¿ki roboczej
WORKDIR /app
#pobranie brakuj¹cych paczek Nugget
RUN dotnet restore
#wykonanie publisha do folderu /publish ustawiaj¹c projekt Foodly.Api jako projekt startowy apliakcji  
RUN dotnet publish ./GraphiCall/GraphiCall/GraphiCall.csproj -o /publish/
#ustawienie folderu /publish jako œcie¿ki roboczej
WORKDIR /publish
#ustawienie komendy uruchamiaj¹cej aplikacjê .Net wraz z ustawieniem zmiennej œrodowiskowej PORT ($ oznacza pocz¹tek nazwy zmiennej œrodowiskowej)
CMD ASPNETCORE_URLS=https://*:$PORT dotnet GraphiCall.dll

