#include "entity2/entitykeyvalues.h"

#ifdef PLATFORM_LINUX
#define PINVOKE_EXPORT __attribute__((visibility("default")))
#else
#define PINVOKE_EXPORT __declspec(dllexport)
#endif

extern "C"
{
  PINVOKE_EXPORT void NativeInitialize(void *ptr);
  PINVOKE_EXPORT void *NativeMakeKeyValue(char *targetMapName);
  PINVOKE_EXPORT void *NativeGetTargetMapName(void *kv);
}