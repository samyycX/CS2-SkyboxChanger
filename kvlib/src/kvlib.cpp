#include "kvlib.h"
#include <iostream>
#include <fstream>
#include <stdio.h>
#include <string.h>
#include <vector.h>
#include <basetypes.h>
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

    void *__cdecl NativeMakeKeyValue(char *fogTargetName, char *vscripts, char *material)
    {
        CEntityKeyValues *kv = new CEntityKeyValues();
        kv->SetString("targetname", fogTargetName);
        kv->SetString("vscripts", vscripts);
        kv->SetString("skyname", material);
        kv->SetString("classname", "env_sky");
        kv->SetString("origin", "0.0 0.0 0.0");
        kv->SetString("angles", "0.0 0.0 0.0");
        kv->SetString("scales", "1.0 1.0 1.0");

        kv->SetBool("useLocalOffset", false);
        kv->SetBool("StartDisabled", false);
        kv->SetFloat("brightnessscale", 1.0f);

        float values[] = {255.0f, 255.0f, 255.0f, 255.0f};
        Vector4D *vec = new Vector4D(values);
        kv->SetVector4D("tint_color", *vec);
        CUtlVector<CEntityKeyValues *> *kvs = new CUtlVector<CEntityKeyValues *>();
        kvs->AddToTail(kv);

        return (void *)kvs;
    }

    // void __cdecl NativeTest(CUtlVector<CEntityKeyValues *> *pEntityKeyValues)
    // {

    //     std::ofstream file;
    //     file.open("E:/testtest.txt", std::ios::app);
    //     file << "COUNT: " << pEntityKeyValues->Count() << "\n";
    //     for (int i = 0; i < pEntityKeyValues->Count(); i++)
    //     {
    //         auto kv = pEntityKeyValues->Element(i);
    //         auto iter = kv->First();
    //         while (kv->IsValidIterator(iter))
    //         {
    //             auto id = kv->GetEntityKeyId(iter);
    //             file << id.GetString() << " = " << kv->GetString(id) << "\n";
    //             iter = kv->Next(iter);
    //         }
    //         if (strcmp(kv->GetString("classname", ""), "env_sky") == 0)
    //         {
    //             kv->SetString("skyname", "materials/skybox/cs_italy_s2_skybox_2_fog.vmat");
    //             file << "SUCCESS" << "\n";
    //         }
    //     }

    //     file.close();
    // }

    // void __cdecl NativeTest2(void *kv)
    // {
    //     CEntityKeyValues *pEntityKeyValues = (CEntityKeyValues *)kv;

    //     std::ofstream file;
    //     file.open("E:/testtest.txt", std::ios::app);
    //     // pEntityKeyValues->Release();
    //     if (strcmp(pEntityKeyValues->GetString("classname", ""), "env_sky") == 0)
    //     {
    //         pEntityKeyValues->SetString("skyname", "wvwvwev");
    //         pEntityKeyValues->SetString("targetname", "123123");
    //         pEntityKeyValues->SetFloat("brightnessscale", 5.0f);
    //         file << "SUCCESS" << "\n";
    //     }
    //     auto iter = pEntityKeyValues->First();
    //     while (pEntityKeyValues->IsValidIterator(iter))
    //     {
    //         auto id = pEntityKeyValues->GetEntityKeyId(iter);

    //         file << id.GetString() << " = " << pEntityKeyValues->GetString(id) << "\n";
    //         iter = pEntityKeyValues->Next(iter);
    //     };

    //     file.close();
    // }

    // void *__cdecl NativeGetTargetMapName(void *kv)
    // {
    //     CEntityKeyValues *pKeyValues = (CEntityKeyValues *)kv;
    //     if (strcmp(pKeyValues->GetString("classname", ""), "skybox_reference") != 0)
    //     {
    //         return (void *)nullptr;
    //     }
    //     return (void *)pKeyValues->GetString("targetMapName", nullptr);
    // }
}