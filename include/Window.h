#include <string>
#include <SDL2/SDL.h>

#pragma once
/**
* Class for application window
*/
class Window
{
private:
	static const unsigned int SCREEN_WIDTH = 1280;
	static const unsigned int SCREEN_HEIGHT = 720;

public:
	Window(const std::string& title);
	virtual ~Window();

	void OnKeyDown(SDL_Keycode, Uint16);
	void OnKeyUp(SDL_Keycode, Uint16);
	void Update();
	bool IsClosed();
	void Clear(float r, float g, float b, float a);
	void IncTime(float value);

	//getters
	unsigned int getWindowWidth();
	unsigned int getWindowHeight();
	bool getTexMode() { return m_texturedMode; }
	bool getAnimMode() { return m_animationMode; }
	bool getSoftShadowsMode() { return m_enableSoftShadows; }
	bool getDOFMode() { return m_enableDOF; }
	int getBouncesNumber() { return m_bouncesNumber; }
	float getAppTime() { return m_time; }
	float getFocusDistance() { return m_focusDistance; }
	bool getRenderingTimeInfo() { return m_renderingTimeInfo; }
	

private:
	SDL_Window* m_window;
	SDL_GLContext m_glContext;
	bool m_isClosed;
	bool m_texturedMode;
	bool m_animationMode;
	bool m_enableSoftShadows;
	int m_bouncesNumber;
	bool m_enableDOF;
	float m_focusDistance;
	SDL_Event e;
	float m_time;
	bool m_renderingTimeInfo;
};

