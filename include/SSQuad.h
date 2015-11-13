#include <GL/glew.h>

/**
* Class representing screen space quad
* (2xtriangle) where we render our scene
*/
class SSQuad
{
public:
	SSQuad(bool init);
	~SSQuad();
	void renderQuad();
	void initQuadVBO();
	GLuint getQuadID();


private:
	GLuint m_quadVBO;
	GLfloat static m_vertices[12];

};

