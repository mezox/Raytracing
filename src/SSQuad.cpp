#include "SSQuad.h"

/// <summary>
/// Quad vertices set-up.
/// </summary>
GLfloat SSQuad::m_vertices[] = { 
	-1.0f, -1.0f,
	1.0f, -1.0f,
	-1.0f, 1.0f,
	-1.0f, 1.0f,
	1.0f, -1.0f,
	1.0f, 1.0f,
};

/// <summary>
/// Constructor.
/// </summary>
/// <param name="init">Initialization handler</param>
SSQuad::SSQuad(bool init)
{
	if (init)
	{
		initQuadVBO();
	}
}

/// <summary>
/// Destructor.
/// </summary>
SSQuad::~SSQuad()
{
}

/// <summary>
/// Sets the quad vbo.
/// </summary>
void SSQuad::initQuadVBO()
{
	glGenBuffers(1, &m_quadVBO);
	glBindBuffer(GL_ARRAY_BUFFER, m_quadVBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(m_vertices), m_vertices, GL_STATIC_DRAW);
}

/// <summary>
/// Renders screen space quad.
/// </summary>
void SSQuad::renderQuad()
{
	// 1st attribute buffer : vertices
	glEnableVertexAttribArray(0);
	{
		glBindBuffer(GL_ARRAY_BUFFER, m_quadVBO);
		glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, 0, (void*)0);

		// Draw the triangles !
		// 2*3 indices starting at 0 -> 2 triangles
		glDrawArrays(GL_TRIANGLES, 0, 6);
	}
	glDisableVertexAttribArray(0);
}


/// <summary>
/// Returns quad ID.
/// </summary>
GLuint SSQuad::getQuadID()
{
	return m_quadVBO;
}
