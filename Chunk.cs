using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public class Chunk : Spatial
{
	public static int CHUNK_WIDTH = 16;
	public static int CHUNK_HEIGHT = 16;
	public static int CHUNK_DEPTH = 16;
	
	private Block.BlockType[,,] _blocks = new Block.BlockType[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_DEPTH];
	
	public Chunk[] neighbours = new Chunk[6];
	
	public static FastNoiseLite _noise = new FastNoiseLite();

	// Start
	public void Init(int chunkX, int chunkY, int chunkZ)
	{
		Translation = new Vector3(chunkX*16,chunkY*16,chunkZ*16);
		GenerateTerrain(chunkX,chunkY,chunkZ);
		GenerateMesh();
	}

	private void GenerateTerrain(int chunkX, int chunkY,int chunkZ)
	{
		for (int x = 0; x < CHUNK_WIDTH; x++)
		{
			for (int z = 0; z < CHUNK_DEPTH; z++)
			{
				float thisNoise = _noise.GetNoise(x+chunkX*CHUNK_WIDTH, z+chunkZ*CHUNK_DEPTH) * 4f;
				for (int y = chunkY * 16; y < chunkY * 16 + CHUNK_HEIGHT && y < thisNoise + 8; y++)
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
	
	public void GenerateMesh()
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

					// Top
					bool hasFace = false;
					if (y == CHUNK_HEIGHT - 1)
					{
						if (neighbours[0] != null && neighbours[0]._blocks[x, 0, z] == Block.BlockType.AIR)
							hasFace = true;
					}
					else if (_blocks[x, y + 1, z] == Block.BlockType.AIR)
						hasFace = true;
					if (hasFace) // Top
						st.AddTriangleFan(new[] {new Vector3(x,y+1,z+1), new Vector3(x,y+1,z), new Vector3(x+1,y+1,z), new Vector3(x+1,y+1,z+1)}, Block.blockDict[_blocks[x,y,z]].GetUVs(0));
					
					// Bottom
					hasFace = false;
					if (y == 0)
					{
						if (neighbours[1] != null && neighbours[1]._blocks[x, CHUNK_HEIGHT - 1, z] == Block.BlockType.AIR)
							hasFace = true;
					}
					else if (_blocks[x, y - 1, z] == Block.BlockType.AIR)
						hasFace = true;
					if (hasFace)
						st.AddTriangleFan(new[] {new Vector3(x,y,z), new Vector3(x,y,z+1), new Vector3(x+1,y,z+1), new Vector3(x+1,y,z)}, Block.blockDict[_blocks[x,y,z]].GetUVs(1));
					
					// Left
					hasFace = false;
					if (x == 0)
					{
						if (neighbours[3] != null && neighbours[3]._blocks[CHUNK_WIDTH - 1, y, z] == Block.BlockType.AIR)
							hasFace = true;
					}
					else if (_blocks[x - 1, y, z] == Block.BlockType.AIR)
						hasFace = true;
					if (hasFace)
						st.AddTriangleFan(new[] {new Vector3(x,y,z), new Vector3(x,y+1,z), new Vector3(x,y+1,z+1), new Vector3(x,y,z+1)}, Block.blockDict[_blocks[x,y,z]].GetUVs(3));

					// Right
					hasFace = false;
					if (x == CHUNK_WIDTH - 1)
					{
						if (neighbours[5] != null && neighbours[5]._blocks[0, y, z] == Block.BlockType.AIR)
							hasFace = true;
					}
					else if (_blocks[x + 1, y, z] == Block.BlockType.AIR)
						hasFace = true;
					if (hasFace)
						st.AddTriangleFan(new[] {new Vector3(x+1,y,z+1), new Vector3(x+1,y+1,z+1), new Vector3(x+1,y+1,z), new Vector3(x+1,y,z)}, Block.blockDict[_blocks[x,y,z]].GetUVs(5));

					// Back
					hasFace = false;
					if (z == 0)
					{
						if (neighbours[4] != null && neighbours[4]._blocks[x, y, CHUNK_DEPTH-1] == Block.BlockType.AIR)
							hasFace = true;
					}
					else if (_blocks[x, y, z - 1] == Block.BlockType.AIR)
						hasFace = true;
					if (hasFace)
						st.AddTriangleFan(new[] {new Vector3(x+1,y,z), new Vector3(x+1,y+1,z), new Vector3(x,y+1,z), new Vector3(x,y,z)}, Block.blockDict[_blocks[x,y,z]].GetUVs(2));
					
					// Front
					if (z == CHUNK_DEPTH - 1)
					{
						if (neighbours[2] != null && neighbours[2]._blocks[x, y, 0] == Block.BlockType.AIR)
							hasFace = true;
					}
					else if (_blocks[x, y, z + 1] == Block.BlockType.AIR)
						hasFace = true;
					if (hasFace)
						st.AddTriangleFan(new[] {new Vector3(x,y,z+1), new Vector3(x,y+1,z+1), new Vector3(x+1,y+1,z+1), new Vector3(x+1,y,z+1)}, Block.blockDict[_blocks[x,y,z]].GetUVs(4));

				}
			}
		}

		st.Index();
		st.GenerateNormals();
		chunkMeshInstance.Mesh = st.Commit();
	}
}
