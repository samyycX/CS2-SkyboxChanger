#include "kvlib.h"
#include <iostream>
#include <stdio.h>
#include <string.h>
#include "entity2/entitykeyvalues.h"

CGameEntitySystem *pGameEntitySystem = nullptr;

CGameEntitySystem *GameEntitySystem()
{
    return pGameEntitySystem;
}

extern "C"
{
    void __cdecl NativeInitialize(void *ptr)
    {
        pGameEntitySystem = (CGameEntitySystem *)ptr;
    }

    void *__cdecl NativeMakeKeyValue(char *targetMapName)
    {
        CEntityKeyValues *kv = new CEntityKeyValues();
        kv->SetString("targetMapName", targetMapName);
        kv->SetString("origin", "0.0 0.0 0.0");
        // kv->SetString("angles", "0.000000 0.000000 0.000000");
        // kv->SetString("scales", "1.000000 1.000000 1.000000");
        kv->SetString("worldgroupid", "skyboxWorldGroup0");
        // kv->SetString("classname", "skybox_reference");
        return (void *)kv;
    }

    void *__cdecl NativeGetTargetMapName(void *kv)
    {
        CEntityKeyValues *pKeyValues = (CEntityKeyValues *)kv;
        if (strcmp(pKeyValues->GetString("classname", ""), "skybox_reference") != 0)
        {
            return (void *)nullptr;
        }
        return (void *)pKeyValues->GetString("targetMapName", nullptr);
    }
}