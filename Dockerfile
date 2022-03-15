FROM debian:buster

RUN apt update && \
    apt install -y wget unzip libxcursor-dev libxinerama-dev libxrandr-dev libxi-dev libgl-dev

COPY ./linux-server .
COPY ./entrypoint.sh entrypoint.sh

ENTRYPOINT [ "./entrypoint.sh" ]