#pragma once

#ifdef NE_PLATFORM_WINDOWS
	#ifdef NE_BUILD_DLL
		#define NE_API __declspec(dllexport)
	#else
		#define NE_API __declspec(dllimport)
	#endif
#else
	#error Native Engine only support windows.
#endif