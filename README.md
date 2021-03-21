# Raymarching Toolkit

This toolkit is a personal project I worked on to hone my skills in 3D graphics and real-time rendering. It is a Unity package that lets you experiment with volumetric shapes in the unity editor.

Users are able to dynamically add primitive volumetric shapes to scenes and perform common operations on them such as subtraction, intersection, and blending. Included is a custom inspector to neatly present values to individual shapes, operations, and the raymarching control itself. In addition, the toolkit enables users to add additional functionality (shapes, operations, filters) with little interaction with pre-existing code.

# Features

## Controller

**Lighting**
- Enable/Disable
- Lambertian model
- Cel Shade model
- Dark Mode

**Filter (Post processing)**
- None (default)
- Highlight (Gradient strength, Emissive Colors)

## Operation
- None (Shape Addition)
- Subtraction
- Intersection
- Blend (Strength, Linear Interpolation)

## Shapes
- Sphere (Radius)
- Box (Dimensions)
- Cone (Height, Ratio)
- Torus (Inner/Outer Radii)
- Rounded Box (Dimensions, Roundness)

## Accreditation

Creating this toolkit would not have been possible without help from the following people

**Sebastian Lague** For his youtube video introducing me to the concept of [Raymarching](https://www.youtube.com/watch?v=Cp5WWtMoeKg).

**Adrian Biagioli** For a basic raymarching implementation [blog](https://adrianb.io/2016/10/01/raymarching.html) that got me started.

**Inigo Quilez** For providing the [mathematical derivations](https://www.iquilezles.org/www/articles/raymarchingdf/raymarchingdf.htm) for volumetric shapes and operations.
