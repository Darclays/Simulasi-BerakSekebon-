using UnityEngine;

public class MeshDrawer : MonoBehaviour
{
    [SerializeField] private Mesh cubeMesh;
    [SerializeField] private Material instancedMaterial;
    [SerializeField] private int instanceCount = 1000;
    [SerializeField] private Vector3 spawnBounds = new Vector3(50f, 50f, 50f);

    private Matrix4x4[][] batches;
    private MaterialPropertyBlock propertyBlock;    // untuk mengubah material tanpa mengubah material asli

    private bool spawned;

    [ContextMenu("Spawn Meshes")]
    private void Spawn()
    {
        if (cubeMesh == null) //untuk mencegah cubemesh tidak null, jika null maka akan membuat cube mesh baru
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temp);
        }

        propertyBlock = new MaterialPropertyBlock();

        // Split instances into batches of max 1023
        int batchSize = 1023;
        int totalBatches = Mathf.CeilToInt((float)instanceCount / batchSize);  //untuk memperkirakan jumlah batch yang dibutuhkan untuk menampung semua instance
        batches = new Matrix4x4[totalBatches][];

        int remaining = instanceCount;
        for (int i = 0; i < totalBatches; i++)
        {
            int currentBatch = Mathf.Min(batchSize, remaining); //untuk menentukan jumlah instance yang akan diisi dalam batch saat ini
            batches[i] = new Matrix4x4[currentBatch]; 

            for (int j = 0; j < currentBatch; j++) //untuk mengurangi jumlah instance yang tersisa untuk batch berikutnya sampai 0
            {
                // logic spawn posisi acak dalam batas spawn
                Vector3 pos = new Vector3(
                    Random.Range(-spawnBounds.x, spawnBounds.x),
                    Random.Range(-spawnBounds.y, spawnBounds.y),
                    Random.Range(-spawnBounds.z, spawnBounds.z)
                );
                batches[i][j] = Matrix4x4.TRS(pos, Quaternion.Euler(0, 0, 0), Vector3.one);
            }
            remaining -= currentBatch;
        }
    
        spawned = true;
    }

    private void Update()
    {
        if (!spawned)
            return;

        if (batches == null || instancedMaterial == null || cubeMesh == null) return;

        for (int i = 0; i < batches.Length; i++)
        {
             if (i < 3)
            {
                Color batchColor = i switch
            {
                0 => Color.red,    // Batch 1
                1 => Color.green,  // Batch 2
                2 => Color.blue,   // Batch 3
                _ => Color.white
            };

            propertyBlock.SetColor("_BaseColor", batchColor);
            }
            else
            {
                propertyBlock.SetColor("_BaseColor", Color.white);
            }
            Graphics.DrawMeshInstanced(
                cubeMesh,
                0,
                instancedMaterial,
                batches[i],
                batches[i].Length,
                propertyBlock
            );
        }
    }
}