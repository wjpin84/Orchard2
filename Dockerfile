FROM ubuntu:14.04
MAINTAINER Jasmin Savard

ENV DOTNET_VERSION=dotnet-dev-1.0.0-preview2-003121 \
  MONO_VERSION=mono-devel

#1 Update and install basic packages needed
RUN sh -c 'echo "deb [arch=amd64] http://apt-mo.trafficmanager.net/repos/dotnet/ trusty main" > /etc/apt/sources.list.d/dotnetdev.list' \
  && apt-key adv --keyserver apt-mo.trafficmanager.net --recv-keys 417A0893 \
  && apt-get update \
  && apt-get install -y gettext zip unzip git uuid-runtime $DOTNET_VERSION $MONO_VERSION

#2 Get Orchard from Github repository & build from source
RUN git clone https://github.com/OrchardCMS/Orchard2.git /home \
  && sh /home/Orchard2/build.sh 

#3 Run Orchard
CMD dotnet run -p /home/Orchard2/src/Orchard.Web

EXPOSE 5000
