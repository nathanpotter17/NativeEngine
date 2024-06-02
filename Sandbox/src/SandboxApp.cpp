#include "Native.h"

class Sandbox : public NativeEngine::Application
{
public:
	Sandbox() {

	}

	~Sandbox() {

	}
};

NativeEngine::Application* NativeEngine::CreateApplication()
{
	return new Sandbox();
}