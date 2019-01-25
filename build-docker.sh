cd GetchMarsRoverPhoto
docker build . -t get-mars-rover-photos
docker images -qf dangling=true | xargs docker rmi
