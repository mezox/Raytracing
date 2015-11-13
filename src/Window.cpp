#include "Window.h"
#include <iostream>
#include <GL/glew.h>

Window::Window(const std::string& title)
{
	SDL_Init(SDL_INIT_EVERYTHING);

	//set color buffers size
	SDL_GL_SetAttribute(SDL_GL_RED_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_GREEN_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_BLUE_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_ALPHA_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_BUFFER_SIZE, 32);

	//enable double buffering
	SDL_GL_SetAttribute(SDL_GL_DOUBLEBUFFER, 1);

	//create window and GL context
	m_window = SDL_CreateWindow(
		title.c_str(),
		SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED,
		SCREEN_WIDTH, SCREEN_HEIGHT,
		SDL_WINDOW_OPENGL | SDL_WINDOW_SHOWN);

	m_glContext = SDL_GL_CreateContext(m_window);

	//init GLEW
	GLenum status = glewInit();
	if (status != GLEW_OK)
	{
		std::cerr << "Glew initialization failed" << std::endl;
	}

	m_texturedMode = false;		//disable textured mode
	m_animationMode = false;	//disable animation mode
	m_time = 0.0f;				//set time to 0
	m_focusDistance = 10.0f;    //set focus distance for depth of field
	m_bouncesNumber = 2;
	m_enableSoftShadows = false;
	m_enableDOF = false;
	m_renderingTimeInfo = false;

	m_isClosed = false;
}

Window::~Window()
{
	SDL_GL_DeleteContext(m_glContext);
	SDL_DestroyWindow(m_window);
	SDL_Quit();
}

void Window::Clear(float r, float g, float b, float a)
{
	glClearColor(r, g, b, a);
	glClear(GL_COLOR_BUFFER_BIT);
}

void Window::OnKeyDown(SDL_Keycode key, Uint16 mod)
{
	switch (key)
	{
	case SDLK_t:
		if (mod == KMOD_LSHIFT)
			m_texturedMode = false;
		else
			m_texturedMode = true;
		break;

	case SDLK_a:
		if (mod == KMOD_LSHIFT)
			m_animationMode = false;
		else
			m_animationMode = true;
		break;

	case SDLK_ESCAPE:
		m_isClosed = true;
		break;

	case SDLK_s:
		if (mod == KMOD_LSHIFT)
			m_enableSoftShadows = false;
		else
			m_enableSoftShadows = true;
		break;

	case SDLK_d:
		if (mod == KMOD_LSHIFT)
			m_enableDOF = false;
		else
			m_enableDOF = true;
		break;

	case SDLK_f:
		if (mod == KMOD_LSHIFT)
			m_renderingTimeInfo = false;
		else
			m_renderingTimeInfo = true;
		break;

	//bounces control
	case SDLK_1:
		m_bouncesNumber = 1;
		break;
	case SDLK_2:
		m_bouncesNumber = 2;
		break;
	case SDLK_3:
		m_bouncesNumber = 3;
		break;
	case SDLK_4:
		m_bouncesNumber = 4;
		break;
	case SDLK_5:
		m_bouncesNumber = 5;
		break;
	case SDLK_6:
		m_bouncesNumber = 6;
		break;
	case SDLK_7:
		m_bouncesNumber = 7;
		break;
	case SDLK_8:
		m_bouncesNumber = 8;
		break;
	case SDLK_9:
		m_bouncesNumber = 9;
		break;
	case SDLK_0:
		m_bouncesNumber = 0;
		break;

	case SDLK_KP_MINUS:
		m_focusDistance -= 0.5;
		break;

	case SDLK_KP_PLUS:
		m_focusDistance += 0.5;
		break;

	default: break;
	}
}

void Window::Update()
{
	SDL_GL_SwapWindow(m_window);

	while (SDL_PollEvent(&e))
	{
		if (e.type == SDL_QUIT)
		{
			m_isClosed = true;
		}

		if (e.type = SDL_KEYDOWN)
		{
			OnKeyDown(e.key.keysym.sym, e.key.keysym.mod);
		}
	}
}

bool Window::IsClosed()
{
	return m_isClosed;
}

unsigned int Window::getWindowWidth()
{
	return SCREEN_WIDTH;
}

unsigned int Window::getWindowHeight()
{
	return SCREEN_HEIGHT;
}

void Window::IncTime(float value)
{
	m_time += value;
}