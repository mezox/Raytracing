# Raytracing
OpenGL raytracer implemented in single GLSL fragment shader. 

Features:

* supports simple geometries (spheres, planes, boxes, triangles)
* textures
* interactive (changing camera position and orientation using mouse)
* animation of sun
* soft shadows
* multisampling 0x-4x
* depth of field
* blur
* multiple ray bounces (selectable)

External dependencies:
* GLEW (linked staticly)
* SDL2

Builded using Visual Studio 2015

![Raytracer](http://i.imgur.com/6JYj4X8.png)
