using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public class Chunk : Node
{
	public static int CHUNK_WIDTH = 16;
	public static int CHUNK_HEIGHT = 16;
	public static int CHUNK_DEPTH = 16;
	
	private Block.BlockType[,,] _blocks = new Block.BlockType[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_DEPTH];
	
	FastNoiseLite _noise = new FastNoiseLite();

	// Start
	public override void _Ready()
	{
		_noise.SetFrequency(0.01f);

		GenerateTerrain();
		GenerateMesh();
	}

	private void GenerateTerrain()
	{
		for (int x = 0; x < CHUNK_WIDTH; x++)
		{
			for (int z = 0; z < CHUNK_DEPTH; z++)
			{
				float thisNoise = _noise.GetNoise(x, z) * 4f;
				for (int y = 0; y < thisNoise + 8; y++)
				{
					_blocks[x, y, z] = Block.BlockType.STONE;
					if (y + 4 > thisNoise + 8)
					{
						_blocks[x, y, z] = Block.BlockType.DIRT;
					}
					if (y + 1 > thisNoise + 8)
					{
						_blocks[x, y, z] = Block.BlockType.GRASS;
					}
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
		
		for (int x = 0; x < CHUNK_WIDTH; x++)
		{
			for (int z = 0; z < CHUNK_DEPTH; z++)
			{
				for (int y = 0; y < CHUNK_HEIGHT; y++)
				{
					if (_blocks[x, y, z] == Block.BlockType.AIR) continue;
					
					if (y == CHUNK_HEIGHT - 1 || _blocks[x,y+1,z] == Block.BlockType.AIR) // Top
					{
						st.AddTriangleFan(new[] {new Vector3(x,y+1,z+1), new Vector3(x,y+1,z), new Vector3(x+1,y+1,z), new Vector3(x+1,y+1,z+1)}, Block.blockDict[_blocks[x,y,z]].GetUVs(0));
					}
					if (y == 0 || _blocks[x,y-1,z] == Block.BlockType.AIR) // Bottom
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z), new Vector3(x,y,z+1), new Vector3(x+1,y,z+1), new Vector3(x+1,y,z)}, Block.blockDict[_blocks[x,y,z]].GetUVs(1));
					}
					if (x == 0 || _blocks[x-1,y,z] == Block.BlockType.AIR) // Left
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z), new Vector3(x,y+1,z), new Vector3(x,y+1,z+1), new Vector3(x,y,z+1)}, Block.blockDict[_blocks[x,y,z]].GetUVs(3));
					}
					if (x == CHUNK_WIDTH - 1 || _blocks[x+1,y,z] == Block.BlockType.AIR) // Right
					{
						st.AddTriangleFan(new[] {new Vector3(x+1,y,z+1), new Vector3(x+1,y+1,z+1), new Vector3(x+1,y+1,z), new Vector3(x+1,y,z)}, Block.blockDict[_blocks[x,y,z]].GetUVs(5));
					}
					if (z == 0 || _blocks[x,y,z-1] == Block.BlockType.AIR) // Back
					{
						st.AddTriangleFan(new[] {new Vector3(x+1,y,z), new Vector3(x+1,y+1,z), new Vector3(x,y+1,z), new Vector3(x,y,z)}, Block.blockDict[_blocks[x,y,z]].GetUVs(2));
					}
					if (z == CHUNK_DEPTH - 1 || _blocks[x,y,z+1] == Block.BlockType.AIR) // Front
					{
						st.AddTriangleFan(new[] {new Vector3(x,y,z+1), new Vector3(x,y+1,z+1), new Vector3(x+1,y+1,z+1), new Vector3(x+1,y,z+1)}, Block.blockDict[_blocks[x,y,z]].GetUVs(4));
					}

				}
			}
		}

		st.Index();
		st.GenerateNormals();
		chunkMeshInstance.Mesh = st.Commit();
	}
}
