#include "Shader.h"
#include <iostream>
#include <fstream>

Shader::Shader(const std::string filename)
{
	m_programID = glCreateProgram();
	m_shaders[0] = CreateShader(LoadShader(filename + ".vs"), GL_VERTEX_SHADER);
	m_shaders[1] = CreateShader(LoadShader(filename + ".fs"), GL_FRAGMENT_SHADER);

	for (unsigned int i = 0; i < NUM_SHADERS; i++)
	{
		glAttachShader(m_programID, m_shaders[i]);
	}

	//glBindAttribLocation(m_programID, 0, "position");

	glLinkProgram(m_programID);
	CheckError(m_programID, GL_LINK_STATUS, true, "Error: Shader program linking failed: ");

	glValidateProgram(m_programID);
	CheckError(m_programID, GL_VALIDATE_STATUS, true, "Error: Shader program validation failed: ");
}

Shader::~Shader()
{
	for (unsigned int i = 0; i < NUM_SHADERS; i++)
	{
		glDetachShader(m_programID, m_shaders[i]);
		glDeleteShader(m_shaders[i]);
	}

	glDeleteProgram(m_programID);
}

GLuint Shader::CreateShader(const std::string& code, unsigned int type)
{
	GLuint shader = glCreateShader(type);

	if (shader == 0)
		std::cerr << "Error compiling shader type " << type << std::endl;

	const GLchar* p[1];
	p[0] = code.c_str();
	GLint lengths[1];
	lengths[0] = code.length();

	glShaderSource(shader, 1, p, lengths);
	glCompileShader(shader);

	CheckError(shader, GL_COMPILE_STATUS, false, "Error compiling shader!");

	return shader;
}

void Shader::Bind()
{
	glUseProgram(m_programID);
}

void Shader::Unbind()
{
	glUseProgram(0);
}

std::string Shader::LoadShader(const std::string& fileName)
{
	std::ifstream file;
	file.open((fileName).c_str());

	std::string output;
	std::string line;

	if (file.is_open())
	{
		while (file.good())
		{
			getline(file, line);
			output.append(line + "\n");
		}
	}
	else
	{
		std::cerr << "Unable to load shader: " << fileName << std::endl;
	}

	return output;
}

void Shader::CheckError(GLuint id, GLuint flag, bool isProgram, const std::string& msg)
{
	GLint success = 0;
	GLchar error[1024] = { 0 };

	if (isProgram)
		glGetProgramiv(id, flag, &success);
	else
		glGetShaderiv(id, flag, &success);

	if (success == GL_FALSE)
	{
		if (isProgram)
			glGetProgramInfoLog(id, sizeof(error), NULL, error);
		else
			glGetShaderInfoLog(id, sizeof(error), NULL, error);

		std::cerr << msg << ": '" << error << "'" << std::endl;
	}
}
