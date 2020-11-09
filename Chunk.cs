using Godot;
using System;

public class Chunk : Node
{
	public static int chunkWidth = 128;
	public static int chunkHeight = 128;
	public static int chunkDepth = 128;
	
	public int[,,] blocks = new int[chunkWidth, chunkHeight, chunkDepth];
	
	FastNoiseLite noise = new FastNoiseLite();

	enum BlockTypes
	{
		AIR,
		STONE,
		DIRT,
		GRASS
	}
	// Start
	public override void _Ready()
	{
		noise.SetFrequency(0.01f);

		GenerateTerrain();
		GenerateMesh();
	}

	private void GenerateTerrain()
	{
		for (int x = 0; x < chunkWidth; x++)
		{
			for (int z = 0; z < chunkDepth; z++)
			{
				float thisNoise = noise.GetNoise(x, z) * 4f;
				for (int y = 0; y < thisNoise + 8; y++)
				{
					blocks[x, y, z] = 1;
				}
			}
		}
	}

	private void GenerateMesh()
	{
		MeshInstance chunkMeshInstance = GetNode<MeshInstance>("ChunkMesh");
		
		SpatialMaterial mat = new SpatialMaterial();
		mat.AlbedoColor = new Color(1,1,1);

		SurfaceTool st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);
		st.SetMaterial(mat);
		
		for (int x = 0; x < chunkWidth; x++)
		{
			for (int z = 0; z < chunkDepth; z++)
			{
				for (int y = 0; y < chunkHeight; y++)
				{
					if (blocks[x, y, z] == 0) continue;
					
					if (y == chunkHeight - 1 || blocks[x,y+1,z] == 0) // Top
					{
						st.AddTriangleFan(new[] {new Vector3(x,y+1,z), new Vector3(x+1,y+1,z), new Vector3(x+1,y+1,z+1), new Vector3(x,y+1,z+1)});
					}
					if (y == 0 || blocks[x,y-1,z] == 0) // Bottom
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z), new Vector3(x,y,z+1), new Vector3(x+1,y,z+1), new Vector3(x+1,y,z)});
					}
					if (x == 0 || blocks[x-1,y,z] == 0) // Left
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z+1), new Vector3(x,y,z), new Vector3(x,y+1,z), new Vector3(x,y+1,z+1)});
					}
					if (x == chunkWidth - 1 || blocks[x+1,y,z] == 0) // Right
					{
						st.AddTriangleFan(new[] {new Vector3(x+1,y,z), new Vector3(x+1,y,z+1), new Vector3(x+1,y+1,z+1), new Vector3(x+1,y+1,z)});
					}
					if (z == 0 || blocks[x,y,z-1] == 0) // Front
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z), new Vector3(x+1,y,z), new Vector3(x+1,y+1,z), new Vector3(x,y+1,z)});
					}
					if (z == chunkDepth - 1 || blocks[x,y,z+1] == 0) // Back
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z+1), new Vector3(x,y+1,z+1), new Vector3(x+1,y+1,z+1), new Vector3(x+1,y,z+1)});
					}

				}
			}
		}

		st.Index();
		st.GenerateNormals();
		chunkMeshInstance.Mesh = st.Commit();
	}
}
