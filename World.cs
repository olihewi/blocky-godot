using Godot;
using System;
using System.Collections.Generic;
using Object = Godot.Object;

public class World : Spatial
{
	[Export]
	public int renderDistance = 8;

	[Export] public int seed = 6895401;
	[Export] public float biomeFrequency = 0.02f;
	[Export] public float biomeJitter = 1;
	
	[Export] public Spatial player;

	private PackedScene chunkScene;
	
	public static List<Biome> biomes = new List<Biome>();
	
	public static FastNoiseLite biomeMap = new FastNoiseLite();

	public static int biomeBlendSize = 2;

	private Godot.Collections.Dictionary<Vector3, Chunk> chunks = new Godot.Collections.Dictionary<Vector3, Chunk>();
	
	public override void _Ready()
	{
		biomeMap = new FastNoiseLite(seed,biomeFrequency,1,FastNoiseLite.NoiseType.Cellular);
		biomeMap.SetCellularJitter(biomeJitter);
		biomeMap.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
		int index = 1;
		foreach (KeyValuePair<Biome.BiomeType,Biome> biome in Biome.biomeDict)
		{
			foreach (FastNoiseLite noiseLayer in biome.Value.noiseLayers)
			{
				noiseLayer.SetSeed((int)(index * seed * 0.75f));
			}
			biomes.Add(biome.Value);
		}
		GD.Print(biomes.Count);
		Chunk._noise.SetFrequency(0.01f);
		chunkScene = (PackedScene) ResourceLoader.Load("res://Chunk.tscn");
		for (int x = 0; x < renderDistance; x++)
		{
			for (int y = 0; y < renderDistance; y++)
			{
				for (int z = 0; z < renderDistance; z++)
				{
					Chunk thisChunk = (Chunk) chunkScene.Instance();
					chunks.Add(new Vector3(x,y,z),thisChunk);
					AddChild(thisChunk);
					thisChunk.Init(x, y, z);
					GetChunkNeighbours(thisChunk,x,y,z);
				}
			}
		}
	}
	

	private void GetChunkNeighbours(Chunk chunk, int chunkX, int chunkY, int chunkZ)
	{
		Chunk neighbourChunk;
		if (chunks.TryGetValue(new Vector3(chunkX, chunkY + 1, chunkZ), out neighbourChunk)) // Top
		{
			neighbourChunk.neighbours[1] = chunk;
			chunk.neighbours[0] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new Vector3(chunkX, chunkY - 1, chunkZ), out neighbourChunk)) // Bottom
		{
			neighbourChunk.neighbours[0] = chunk;
			chunk.neighbours[1] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new Vector3(chunkX, chunkY, chunkZ + 1), out neighbourChunk)) // Front
		{
			neighbourChunk.neighbours[4] = chunk;
			chunk.neighbours[2] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new Vector3(chunkX, chunkY, chunkZ - 1), out neighbourChunk)) // Back
		{
			neighbourChunk.neighbours[2] = chunk;
			chunk.neighbours[4] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new Vector3(chunkX - 1, chunkY, chunkZ), out neighbourChunk)) // Left
		{
			neighbourChunk.neighbours[5] = chunk;
			chunk.neighbours[3] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new Vector3(chunkX + 1, chunkY, chunkZ), out neighbourChunk)) // Right
		{
			neighbourChunk.neighbours[3] = chunk;
			chunk.neighbours[5] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
