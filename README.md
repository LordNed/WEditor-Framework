# WEditor-Framework
In-progress C#/OpenTK Framework designed to remove the boilerplate of OpenTK setup, keyboard/mouse input, basic rendering, etc.

Consists of two projects. 

* WEditor is a windowless, context-less "editor core" that you pipe information into after initializing it. WEditor will handle the back-end side of things for you - entity management, camera/render system management, etc.
* TestEditor is an example usage of WEditor that pipes in the necessary information from a OpenTK/WinForm control.

It's pretty in-progress but stands as the back-end to most of my modding tools (because I'm tired fo re-writing them) so hopefully will be of use to someone else.
