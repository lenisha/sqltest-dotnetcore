FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1

RUN groupadd --gid 3000 odbc \
  && useradd --uid 1000 --gid odbc --shell /bin/bash --create-home odbc

# Update CURL
#RUN echo "deb http://deb.debian.org/debian buster main" |  tee -a /etc/apt/sources.list

# install ODBC driver on Debian
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl gnupg2 \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/debian/9/prod.list > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get install -y --no-install-recommends locales apt-transport-https \
    && echo "en_US.UTF-8 UTF-8" > /etc/locale.gen && locale-gen \
    && apt-get update && ACCEPT_EULA=Y apt-get -y --no-install-recommends install unixodbc  \
   # msodbcsql17 mssql-tools unixodbc-dev \
    && rm -rf /var/lib/apt/lists/* 

# Install troubleshooting tools
# RUN apt-get remove -y curl
# RUN apt-get update \
#    && apt-get install -y traceroute wget dnsutils netcat-openbsd jq nmap \ 
#                          net-tools git openssh-client   \
#    && rm -rf /var/lib/apt/lists/*
# RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

COPY msodbcsql_17.5.1.2-1_amd64.deb ./msodbcsql_17.5.1.2-1_amd64.deb
RUN ACCEPT_EULA=Y apt-get  -y --no-install-recommends  install ./msodbcsql_17.5.1.2-1_amd64.deb

# Trace enabled 
#COPY odbcinst.ini /etc

# Install app
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "CloudSQL.dll"]