#pobranie obrazu .net sdk 5.0 
FROM mcr.microsoft.com/dotnet/sdk:8.0
#skopiowanie plik�w i folder�w z obecnej �cie�ki do folderu /app w powstaj�cym obrazie
COPY . /app
#ustawienie folderu /app w powstaj�cym obrazie jako �cie�ki roboczej
WORKDIR /app
#pobranie brakuj�cych paczek Nugget
RUN dotnet restore
#wykonanie publisha do folderu /publish ustawiaj�c projekt Foodly.Api jako projekt startowy apliakcji  
RUN dotnet publish ./GraphiCall/GraphiCall/GraphiCall.csproj -o /publish/
#ustawienie folderu /publish jako �cie�ki roboczej
WORKDIR /publish
#ustawienie komendy uruchamiaj�cej aplikacj� .Net wraz z ustawieniem zmiennej �rodowiskowej PORT ($ oznacza pocz�tek nazwy zmiennej �rodowiskowej)
CMD ASPNETCORE_URLS=https://*:$PORT dotnet GraphiCall.dll

