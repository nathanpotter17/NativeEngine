#pragma once


#ifdef NE_PLATFORM_WINDOWS

extern NativeEngine::Application* NativeEngine::CreateApplication();

int main(int argc, char** argv)
{	
	printf("This is a Native Engine Application");
	auto app = NativeEngine::CreateApplication();
	app->Run();
	delete app;
}

#endif