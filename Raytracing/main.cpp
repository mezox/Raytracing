#include <GL/glew.h>
#include <glm/glm.hpp>
#include <stdlib.h>
#include <iostream>
#include <SDL2/SDL.h>
#undef main

#include "Window.h"
#include "Shader.h"
#include "SSQuad.h"
#include "Texture.h"

GLuint appTime;
GLuint focusDistance;
GLuint w, h;
GLuint mouseX, mouseY;
GLuint texturedMode = 0;
GLint64 timerStart;
GLint64 timerEnd;
GLuint softShadowMode = 0;
GLuint DOFMode = 0;
GLuint numberBounces = 2;
int numberBouncesOld = 0;

int main(int argc, char ** argv)
{
		//create window
		Window win("Raytracing in Shader");

		//load & create raytracing shader
		Shader simple("./simple");

		//create Quad object and generate VBO
		SSQuad quad(true);

		//bind default framebuffer
		glBindFramebuffer(GL_FRAMEBUFFER, 0);

		//texture
		GLuint tex, sphereTex;
		Texture * floor = new Texture(GL_TEXTURE_2D, "concrete.jpg");
		Texture * map = new Texture(GL_TEXTURE_2D, "map.jpg");

		if (!floor->Load(true, false) || !map->Load(true, true))
		{
			printf("Error loading diff. texture \n");
			delete floor;
			delete map;
			floor = NULL;
			map = NULL;
		}

		tex = glGetUniformLocation(simple.getProgramID(), "tex");
		sphereTex = glGetUniformLocation(simple.getProgramID(), "map");
		appTime = glGetUniformLocation(simple.getProgramID(), "iGlobalTime");
		w = glGetUniformLocation(simple.getProgramID(), "iWidth");
		h = glGetUniformLocation(simple.getProgramID(), "iHeight");
		mouseX = glGetUniformLocation(simple.getProgramID(), "iMouseX");
		mouseY = glGetUniformLocation(simple.getProgramID(), "iMouseY");
		texturedMode = glGetUniformLocation(simple.getProgramID(), "iTexturedMode");
		focusDistance = glGetUniformLocation(simple.getProgramID(), "iFocusDistance");
		softShadowMode = glGetUniformLocation(simple.getProgramID(), "iSoftShadowMode");
		DOFMode = glGetUniformLocation(simple.getProgramID(), "iDOFMode");
		numberBounces = glGetUniformLocation(simple.getProgramID(), "iNumberBounces");

		int  mx = -1;
		int my = -1;

		unsigned int i = 0;
		double frameRenderingTime;
		bool softShadowsOn = false;
		bool DOFOn= false;
		bool texturesOn = false;
		bool animationOn = false;

		//printf("%s\n", glGetString(GL_VERSION));

		while (!win.IsClosed())
		{
			//get cursor position relative to window
			SDL_GetMouseState(&mx, &my);

			win.Clear(0.5f, 0.5f, 0.5f, 1.0f);
			glViewport(0, 0, 1280, 720);

			//render quad
			simple.Bind();
			{
				glUniform1f(appTime, win.getAppTime());
				glUniform1f(focusDistance, win.getFocusDistance());
				glUniform1i(w, 1280);
				glUniform1i(h, 720);
				glUniform1i(mouseX, mx);
				glUniform1i(mouseY, my);

				if (win.getTexMode())
				{
					glUniform1i(tex, 0);
					glUniform1i(sphereTex, 1);
					glUniform1i(texturedMode, 1);

					floor->Bind(GL_TEXTURE0);
					map->Bind(GL_TEXTURE1);
					if(!texturesOn) {
						printf("TEXTURES: ON\n");
					}
					texturesOn = true;
				}
				else
				{
					glUniform1i(texturedMode, 0);
					if(texturesOn) {
						printf("TEXTURES: OFF\n");
					}
					texturesOn = false;
				}

				if (win.getSoftShadowsMode())
				{
					glUniform1i(softShadowMode, 1);
					if(!softShadowsOn) {
						printf("SOFT SHADOWS: ON\n");
					}
					softShadowsOn = true;
				}
				else
				{
					glUniform1i(softShadowMode, 0);
					if(softShadowsOn) {
						printf("SOFT SHADOWS: OFF\n");
					}
					softShadowsOn = false;
				}

				if (win.getDOFMode())
				{
					glUniform1i(DOFMode, 1);
					if(!DOFOn) {
						printf("DEPTH OF FIELD: ON\n");
					}
					DOFOn = true;
				}
				else
				{
					glUniform1i(DOFMode, 0);
					if(DOFOn) {
						printf("DEPTH OF FIELD: OFF\n");
					}
					DOFOn = false;
				}

				if (win.getBouncesNumber() != numberBouncesOld) {
					numberBouncesOld = win.getBouncesNumber();
					glUniform1i(numberBounces, win.getBouncesNumber());
					printf("BOUNCES: %d\n", numberBouncesOld);
				}

				glGetInteger64v(GL_TIMESTAMP, &timerStart);
				
				quad.renderQuad();
				
			}
			simple.Unbind();
			
			win.Update();
			
			++i;
			glGetInteger64v(GL_TIMESTAMP, &timerEnd);
			if(win.getRenderingTimeInfo())
			{
				frameRenderingTime = (timerEnd - timerStart) / 1000000.0;  // time for this frame rendering in ms
				printf("Frame no. %d rendering time: %.0fms\n", i, frameRenderingTime);
				printf("FPS: %.2f\n", 1000.0 / frameRenderingTime);
			}

			/* // print windows setup
			{
				printf("Current setup:\n");
				printf("\tTextures: ");
				(texturesOn) ? printf("ON\n") : printf("OFF\n");
				printf("\tSoft shadows: ");
				(softShadowsOn) ? printf("ON\n") : printf("OFF\n");
				printf("\tDepth of field: ");
				(DOFOn) ? printf("ON\n") : printf("OFF\n");
				printf("\tNumber of bounces: %d\n\n", numberBouncesOld);
				printf("Frame no. %d\n\n", i);
				win.setSetupInfoToFalse();
			}*/
			
			if (win.getAnimMode())
			{
				win.IncTime(0.01f);
				
				if(!animationOn) {
					printf("ANIMATION: ON\n");
				}
				animationOn = true;
			}
			else
			{
				if(animationOn) {
					printf("ANIMATION: OFF\n");
				}
				animationOn = false;
			}
		}

		return 0;
};