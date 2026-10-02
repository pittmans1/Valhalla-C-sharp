using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropFragmentPoolReference : MonoBehaviour
{
    public GameObject sourcePrefab;
}

public class PropFragmentPool : MonoBehaviour
{
    private readonly Dictionary<ulong, Queue<GameObject>> pooledFragments = new Dictionary<ulong, Queue<GameObject>>();

    public GameObject GetFragments(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return null;
        }

        ulong prefabId = EntityId.ToULong(prefab.GetEntityId());
        GameObject fragmentRoot;

        if (pooledFragments.TryGetValue(prefabId, out Queue<GameObject> queue) && queue.Count > 0)
        {
            fragmentRoot = queue.Dequeue();
            fragmentRoot.transform.SetPositionAndRotation(position, rotation);
            fragmentRoot.SetActive(true);
            return fragmentRoot;
        }

        fragmentRoot = Instantiate(prefab, position, rotation);
        fragmentRoot.SetActive(true);

        var reference = fragmentRoot.GetComponent<PropFragmentPoolReference>();
        if (reference == null)
        {
            reference = fragmentRoot.AddComponent<PropFragmentPoolReference>();
        }

        reference.sourcePrefab = prefab;

        return fragmentRoot;
    }

    public void ReturnFragments(GameObject fragmentRoot, float delay = 4f)
    {
        if (fragmentRoot == null)
        {
            return;
        }

        StartCoroutine(ReturnFragmentsRoutine(fragmentRoot, delay));
    }

    private IEnumerator ReturnFragmentsRoutine(GameObject fragmentRoot, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (fragmentRoot == null)
        {
            yield break;
        }

        var poolReference = fragmentRoot.GetComponent<PropFragmentPoolReference>();
        if (poolReference == null || poolReference.sourcePrefab == null)
        {
            Destroy(fragmentRoot);
            yield break;
        }

        ulong prefabId = EntityId.ToULong(poolReference.sourcePrefab.GetEntityId());
        if (!pooledFragments.ContainsKey(prefabId))
        {
            pooledFragments[prefabId] = new Queue<GameObject>();
        }

        fragmentRoot.SetActive(false);
        foreach (var rigidBody in fragmentRoot.GetComponentsInChildren<Rigidbody>())
        {
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        pooledFragments[prefabId].Enqueue(fragmentRoot);
    }
}
