using Godot;
using System;

public class Chunk : Node
{
	public static int chunkWidth = 16;
	public static int chunkHeight = 16;
	public static int chunkDepth = 16;
	
	private BlockType[,,] blocks = new BlockType[chunkWidth, chunkHeight, chunkDepth];
	
	FastNoiseLite noise = new FastNoiseLite();

	private enum BlockType
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
					blocks[x, y, z] = BlockType.STONE;
				}
			}
		}
	}

	private void GenerateMesh()
	{
		MeshInstance chunkMeshInstance = GetNode<MeshInstance>("ChunkMesh");
		
		SpatialMaterial mat = (SpatialMaterial)ResourceLoader.Load("res://atlas.tres");

		SurfaceTool st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);
		st.SetMaterial(mat);
		
		for (int x = 0; x < chunkWidth; x++)
		{
			for (int z = 0; z < chunkDepth; z++)
			{
				for (int y = 0; y < chunkHeight; y++)
				{
					if (blocks[x, y, z] == BlockType.AIR) continue;
					
					if (y == chunkHeight - 1 || blocks[x,y+1,z] == BlockType.AIR) // Top
					{
						st.AddTriangleFan(new[] {new Vector3(x,y+1,z), new Vector3(x+1,y+1,z), new Vector3(x+1,y+1,z+1), new Vector3(x,y+1,z+1)}, getSurfaceUVs(blocks[x,y,z]));
					}
					if (y == 0 || blocks[x,y-1,z] == BlockType.AIR) // Bottom
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z), new Vector3(x,y,z+1), new Vector3(x+1,y,z+1), new Vector3(x+1,y,z)}, getSurfaceUVs(blocks[x,y,z]));
					}
					if (x == 0 || blocks[x-1,y,z] == BlockType.AIR) // Left
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z+1), new Vector3(x,y,z), new Vector3(x,y+1,z), new Vector3(x,y+1,z+1)}, getSurfaceUVs(blocks[x,y,z]));
					}
					if (x == chunkWidth - 1 || blocks[x+1,y,z] == BlockType.AIR) // Right
					{
						st.AddTriangleFan(new[] {new Vector3(x+1,y,z), new Vector3(x+1,y,z+1), new Vector3(x+1,y+1,z+1), new Vector3(x+1,y+1,z)}, getSurfaceUVs(blocks[x,y,z]));
					}
					if (z == 0 || blocks[x,y,z-1] == BlockType.AIR) // Front
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z), new Vector3(x+1,y,z), new Vector3(x+1,y+1,z), new Vector3(x,y+1,z)}, getSurfaceUVs(blocks[x,y,z]));
					}
					if (z == chunkDepth - 1 || blocks[x,y,z+1] == BlockType.AIR) // Back
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z+1), new Vector3(x,y+1,z+1), new Vector3(x+1,y+1,z+1), new Vector3(x+1,y,z+1)}, getSurfaceUVs(blocks[x,y,z]));
					}

				}
			}
		}

		st.Index();
		st.GenerateNormals();
		chunkMeshInstance.Mesh = st.Commit();
	}

	private Vector2[] getSurfaceUVs(BlockType blockType)
	{
		switch (blockType)
		{
			case BlockType.STONE:
				return new Vector2[]
				{
					new Vector2(0.0625f,1),
					new Vector2(0.125f,1),
					new Vector2(0.125f,0.9375f),
					new Vector2(0.0625f,0.9375f),
				};
			default:
				return new Vector2[]
				{
					new Vector2(0,0),
					new Vector2(0,0.0625f),
					new Vector2(0.0625f,0.0625f),
					new Vector2(0.0625f,0), 
				};
		}
	}
}
