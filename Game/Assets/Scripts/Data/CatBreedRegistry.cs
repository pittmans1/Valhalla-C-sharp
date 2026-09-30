using System.Collections.Generic;

public static class CatBreedRegistry
{
    private static Dictionary<int, CatBreedData> registry;

    public static Dictionary<int, CatBreedData> GetMasterDatabase()
    {
        if (registry != null) return registry;

        registry = new Dictionary<int, CatBreedData>();

        #region == BLOCK 100: STANDARD ARCHETYPES ==
        AddEntry(100, "Orange Chaos",       1.0f,  1.0f,  1.0f);
        AddEntry(101, "Fat Tuxedo",        1.1f,  0.9f,  1.2f);
        AddEntry(102, "Leopard Spotted",    0.9f,  1.1f,  1.0f);
        AddEntry(103, "Siamese Singer",     0.8f,  1.2f,  0.9f);
        AddEntry(104, "Midnight Black",     1.0f,  1.0f,  1.1f);
        #endregion

        #region == BLOCK 200: KITTEN VARIATIONS (FAST, ULTRA-LIGHT) ==
        AddEntry(200, "Orange Kitten",      0.4f,  1.5f,  0.3f);
        AddEntry(201, "Tuxedo Kitten",      0.45f, 1.4f,  0.4f);
        AddEntry(202, "Calico Calamity",    0.4f,  1.6f,  0.3f);
        AddEntry(203, "Scottish Fold Baby", 0.5f,  1.3f,  0.5f);
        AddEntry(204, "Sphynx Wrinkles",    0.35f, 1.7f,  0.2f);
        #endregion

        #region == BLOCK 300: FAT BOY ARCHETYPES (MASSIVE WEIGHT, LOW SPEED, HIGH IMPACT) ==
        AddEntry(300, "Chonky Garfield",    2.2f,  0.6f,  2.0f);
        AddEntry(301, "The Absolute Unit",  2.5f,  0.5f,  2.5f);
        AddEntry(302, "Fluffy Pillow",      2.0f,  0.7f,  1.8f);
        AddEntry(303, "Sausage Roll",       2.1f,  0.65f, 1.9f);
        #endregion

        #region == BLOCK 400: SHRUNK-DOWN APEX PREDATORS (ULTIMATE DESTRUCTION) ==
        AddEntry(400, "Pocket Lion",        1.8f,  1.2f,  3.0f);
        AddEntry(401, "Micro Tiger",        1.7f,  1.3f,  3.2f);
        AddEntry(402, "Miniature Cheetah",   1.2f,  2.0f,  2.5f);
        AddEntry(403, "Desk Jaguar",        1.6f,  1.4f,  3.1f);
        AddEntry(404, "Toybox Snow Leopard",1.5f,  1.5f,  2.9f);
        #endregion

        return registry;
        // Optimization: Prefab models can be bound programmatically inside your 
        // PlayerSpawner using an asset address mapping table to minimize boot memory consumption.
    }

    private static void AddEntry(int id, string breedName, float weight, float speed, float power)
    {
        CatBreedData data = UnityEngine.ScriptableObject.CreateInstance<CatBreedData>();
        data.catID = id;
        data.catName = breedName;
        data.attributes.weightModifier = weight;
        data.attributes.speedModifier = speed;
        data.attributes.destructionPower = power;

        registry.Add(id, data);
    }
}