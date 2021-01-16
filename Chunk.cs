using Godot;
using System;
using System.Collections.Generic;

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
		Biome[,] biomeBlend = new Biome[CHUNK_WIDTH + 2 * World.biomeBlendSize,CHUNK_DEPTH + 2 * World.biomeBlendSize];
		for (int x = 0; x < CHUNK_WIDTH + 2 * World.biomeBlendSize; x++)
		{
			for (int z = 0; z < CHUNK_DEPTH + 2 * World.biomeBlendSize; z++)
			{
				biomeBlend[x, z] = GetBiome(chunkX * CHUNK_WIDTH + x - World.biomeBlendSize, chunkZ * CHUNK_DEPTH + z - World.biomeBlendSize);
			}
		}
		
		for (int x = 0; x < CHUNK_WIDTH; x++)
		{
			for (int z = 0; z < CHUNK_DEPTH; z++)
			{
				Biome thisBiome = GetBiome(chunkX * CHUNK_WIDTH + x, chunkZ * CHUNK_DEPTH + z);
				Dictionary<Biome,int> thisBlend = new Dictionary<Biome,int>();
				int numOfBiomes = 0;
				for (int blendX = x; blendX <= x + World.biomeBlendSize * 2; blendX++)
				{
					for (int blendZ = z; blendZ <= z + World.biomeBlendSize * 2; blendZ++)
					{
						numOfBiomes++;
						if (thisBlend.ContainsKey(biomeBlend[blendX, blendZ]))
						{
							thisBlend[biomeBlend[blendX, blendZ]]++;
							
						}
						else
						{
							thisBlend.Add(biomeBlend[blendX, blendZ], 1);
						}
					}
				}

				float heightMap = 0;
				foreach (KeyValuePair<Biome, int> blend in thisBlend)
				{
					float thisBlendHeightMap = 0;
					foreach (FastNoiseLite noiseLayer in blend.Key.noiseLayers)
					{
						thisBlendHeightMap += noiseLayer.GetNoise(chunkX * CHUNK_WIDTH + x, chunkZ * CHUNK_DEPTH + z) * noiseLayer.mAmplitude;
					}
					thisBlendHeightMap += blend.Key.surfaceHeight;
					heightMap += thisBlendHeightMap * blend.Value;
				}
				heightMap /= numOfBiomes;

				//float thisNoise = _noise.GetNoise(x+chunkX*CHUNK_WIDTH, z+chunkZ*CHUNK_DEPTH) * 4f;
				for (int y = 0; y < CHUNK_HEIGHT; y++)
				{
					if (chunkY * CHUNK_HEIGHT + y > heightMap + 4) continue;
					_blocks[x, y, z] = thisBiome.fillerBlock;
					if (_blocks[x,y,z] == Block.BlockType.TERRACOTTA)
					{
						if (y%4 == 0)
						{
							_blocks[x,y,z] = Block.BlockType.TERRACOTTA;
						}
						else if (y%4 == 1)
						{
							_blocks[x,y,z] = Block.BlockType.RED_TERRACOTTA;
						}
						else if (y%4 == 2)
						{
							_blocks[x,y,z] = Block.BlockType.YELLOW_TERRACOTTA;
						}
						else if (y%4 == 3)
						{
							_blocks[x,y,z] = Block.BlockType.WHITE_TERRACOTTA;
						}
					}
					foreach (BlockLayer layer in thisBiome.blockLayers)
					{
						if (chunkY * CHUNK_HEIGHT + y + layer.height > heightMap + 4)
						{
							_blocks[x, y, z] = layer.block;
						}
					}

					/*if (chunkY * CHUNK_HEIGHT + y + 1 > heightMap + 4)
					{
						GenerateTreeInChunk(x, y, z, biomeBlend[x + World.biomeBlendSize,z + World.biomeBlendSize]);
					}*/
				}
			}
		}
	}

	private void GenerateTreeInChunk(int x, int y, int z, Biome biome)
	{
		if (biome.features.Count == 0) return;
		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Seed = (ulong) (x * 10000 + z);
		FeatureBiomeInstance thisTree = biome.features[Mathf.Abs(x + y + z) % biome.features.Count];
		if (!(rng.Randf() < thisTree.probability)) return;
		Feature thisFeature = Feature.featureDict[thisTree.feature];
		int treeHeight = Mathf.FloorToInt(rng.Randf() * (thisFeature.heightMinMax.y - thisFeature.heightMinMax.x) + thisFeature.heightMinMax.x);
		int leavesHeight = treeHeight - 1;
		for (int _y = 0; _y < treeHeight; y++)
		{
			for (int _x = -thisFeature.leavesWidth; _x < thisFeature.trunkWidth + thisFeature.leavesWidth; _x++)
			{
				for (int _z = -thisFeature.leavesWidth; _z < thisFeature.trunkWidth + thisFeature.leavesWidth; _z++)
				{
					if (_y < treeHeight - 1)
					{
						SetBlock(x + _x,y + _y + 1, z + _z, thisFeature.leaves);
					}
				}
			}
			for (int _x = 0; _x < thisFeature.trunkWidth; _x++)
			{
				for (int _z = 0; _z < thisFeature.trunkWidth; _z++)
				{
					SetBlock(x + _x,y+_y+1,z + _z,thisFeature.bark);
				}
			}
		}
	}

	private void SetBlock(int x, int y, int z, Block.BlockType block)
	{
		Chunk chunk = this;
		if (x < 0 || x > CHUNK_WIDTH - 1 || y < 0 || y > CHUNK_HEIGHT - 1 || z < 0 || z > CHUNK_DEPTH - 1) return;
		chunk._blocks[x, y, z] = block;
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
					hasFace = false;
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

	private Biome GetBiome(int x, int z)
	{
		float thisValue = World.biomeMap.GetNoise(x, z);
		Biome thisBiome = World.biomes[Mathf.FloorToInt(thisValue * World.biomes.Count)];
		return thisBiome;
	}
}
