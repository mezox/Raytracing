OBJ = main.o Shader.o SSQuad.o Texture.o Window.o
CC = g++
CFLAGS = -Wall
LIBRARIES = -lGL -lGLU -lGLEW -lglut -lSDL2 -lm

all: pgr

pgr : $(OBJ)
	$(CC) $(OBJ) $(CFLAGS) $(LIBRARIES) -o pgr

main.o : main.cpp Window.h Shader.h SSQuad.h Texture.h
	g++ -c main.cpp

Shader.o : Shader.cpp Shader.h
	$(CC) -c Shader.cpp

SSQuad.o : SSQuad.cpp SSQuad.h
	$(CC) -c SSQuad.cpp

Texture.o : Texture.cpp Texture.h stb_image.c
	$(CC) -c Texture.cpp

Window.o : Window.cpp Window.h
	$(CC) -c Window.cpp

clean:
	rm *.o pgr
