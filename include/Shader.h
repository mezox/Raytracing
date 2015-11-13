#include <string>
#include <GL/glew.h>

#pragma once
/**
* Shader class, loads shader code from file
* for both Vertex and Fragment shader, creates
* shaders, links and compiles them into Shader Program
*/
class Shader
{
public:
	Shader(const std::string filename);
	virtual ~Shader();

	static std::string LoadShader(const std::string& filename);
	static GLuint CreateShader(const std::string &code, GLenum shaderType);
	static void CheckError(GLuint id, GLuint flag, bool isProgram, const std::string& msg);
	GLuint getProgramID() { return m_programID; }

	void Bind();
	void Unbind();

private:
	static const unsigned int NUM_SHADERS = 2;
	GLuint m_programID;
	GLuint m_shaders[NUM_SHADERS];
};

