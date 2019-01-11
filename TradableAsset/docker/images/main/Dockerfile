# Reference base image
FROM unimaginaryunit/tradableasset-base:latest

# Copy all files needed
WORKDIR /app
RUN mkdir source
RUN mkdir test
COPY ./source /app/source
COPY ./test /app/test

# Restore & publish
WORKDIR /app/source
RUN dotnet publish -c Debug TradableAsset.sln

# Load python stage
FROM python:3.7.2-stretch
WORKDIR /app
COPY --from=0 /app .
CMD [ "bash" ]