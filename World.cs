using Godot;
using System;
using Godot.Collections;
using Object = Godot.Object;

public class World : Spatial
{
	[Export]
	public int renderDistance = 8;

	private PackedScene chunkScene;

	private Dictionary<ChunkPos, Chunk> chunks = new Dictionary<ChunkPos, Chunk>();
	
	public override void _Ready()
	{
		Chunk._noise.SetFrequency(0.01f);
		
		chunkScene = (PackedScene) ResourceLoader.Load("res://Chunk.tscn");
		for (int x = 0; x < renderDistance; x++)
		{
			for (int y = 0; y < 1; y++)
			{
				for (int z = 0; z < renderDistance; z++)
				{
					Chunk thisChunk = (Chunk) chunkScene.Instance();
					chunks.Add(new ChunkPos(x,y,z),thisChunk);
					AddChild(thisChunk);
					
					GetChunkNeighbours(thisChunk,x,y,z);
					thisChunk.Init(x, y, z);
				}
			}
		}
	}

	private void GetChunkNeighbours(Chunk chunk, int chunkX, int chunkY, int chunkZ)
	{
		Chunk neighbourChunk;
		if (chunks.TryGetValue(new ChunkPos(chunkX, chunkY + 1, chunkZ), out neighbourChunk)) // Top
		{
			neighbourChunk.neighbours[1] = chunk;
			chunk.neighbours[0] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new ChunkPos(chunkX, chunkY - 1, chunkZ), out neighbourChunk)) // Bottom
		{
			neighbourChunk.neighbours[0] = chunk;
			chunk.neighbours[1] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new ChunkPos(chunkX, chunkY, chunkZ - 1), out neighbourChunk)) // Front
		{
			neighbourChunk.neighbours[4] = chunk;
			chunk.neighbours[2] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new ChunkPos(chunkX, chunkY, chunkZ + 1), out neighbourChunk)) // Back
		{
			neighbourChunk.neighbours[2] = chunk;
			chunk.neighbours[4] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new ChunkPos(chunkX - 1, chunkY, chunkZ), out neighbourChunk)) // Left
		{
			neighbourChunk.neighbours[5] = chunk;
			chunk.neighbours[3] = neighbourChunk;
			neighbourChunk.GenerateMesh();
		}
		if (chunks.TryGetValue(new ChunkPos(chunkX + 1, chunkY, chunkZ), out neighbourChunk)) // Right
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

public class ChunkPos : Object
{
	public ChunkPos(int _x, int _y, int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public int x;
	public int y;
	public int z;
}
