using Godot;
using System;
using System.Collections.Generic;

public class Biome : Godot.Object
{
	public List<FastNoiseLite> noiseLayers = new List<FastNoiseLite>();
	public List<BlockLayer> blockLayers = new List<BlockLayer>();
	public int surfaceHeight = 0;
	public Block.BlockType fillerBlock = Block.BlockType.STONE;
	public List<FeatureBiomeInstance> features = new List<FeatureBiomeInstance>();

	public Biome()
	{
		noiseLayers.Add(new FastNoiseLite());
	}

	public Biome(Block.BlockType _fillerBlock, List<BlockLayer> _blockLayers, List<FastNoiseLite> _noiseLayers, List<FeatureBiomeInstance> _features, int _surfaceHeight = 0)
	{
		fillerBlock = _fillerBlock;
		surfaceHeight = _surfaceHeight;
		int index = 1;
		foreach (FastNoiseLite noiseLayer in _noiseLayers)
		{
			noiseLayers.Add(noiseLayer);
			index++;
		}
		features.AddRange(_features);
		blockLayers.AddRange(_blockLayers);
	}

	public float GetHeightMap(int x, int z)
	{
		float heightMap = 0;
		foreach (FastNoiseLite noiseLayer in noiseLayers)
		{
			heightMap += noiseLayer.GetNoise(x, z) * noiseLayer.mAmplitude;
		}
		return heightMap + surfaceHeight;
	}

	public static Dictionary<BiomeType,Biome> biomeDict = new Dictionary<BiomeType,Biome>()
	{
		// Plains
		{BiomeType.PLAINS,new Biome(Block.BlockType.STONE, new List<BlockLayer>()
		{ // Block Layers
			new BlockLayer(Block.BlockType.DIRT,4),
			new BlockLayer(Block.BlockType.GRASS,1)
		}, new List<FastNoiseLite>()
		{ // Noise Layers
			new FastNoiseLite(0,0.02f, 8)
		}, new List<FeatureBiomeInstance>()
		{ // Features
			new FeatureBiomeInstance(Feature.FeatureType.OAK_TREE,0.002f)
		})},
		// Forest
		{BiomeType.FOREST,new Biome(Block.BlockType.STONE,new List<BlockLayer>()
		{ // Block Layers
			new BlockLayer(Block.BlockType.DIRT,4),
			new BlockLayer(Block.BlockType.GRASS,1)
		}, new List<FastNoiseLite>()
		{ // Noise Layers
			new FastNoiseLite(0,0.02f, 4),
			new FastNoiseLite(0,0.005f,16)
		}, new List<FeatureBiomeInstance>()
		{ // Features
			new FeatureBiomeInstance(Feature.FeatureType.OAK_TREE,0.01f),
			new FeatureBiomeInstance(Feature.FeatureType.BIRCH_TREE, 0.001f)
		})},
		// Desert
		{BiomeType.DESERT,new Biome(Block.BlockType.STONE, new List<BlockLayer>()
		{ // Block Layers
			new BlockLayer(Block.BlockType.SANDSTONE,10),
			new BlockLayer(Block.BlockType.SAND,3)
		}, new List<FastNoiseLite>()
		{ // Noise Layers
			new FastNoiseLite(0,0.02f, 10)
		}, new List<FeatureBiomeInstance>()
		{ // Features
			new FeatureBiomeInstance(Feature.FeatureType.CACTUS,0.002f)
		})},
		// Mesa
		{BiomeType.MESA,new Biome(Block.BlockType.TERRACOTTA, new List<BlockLayer>(), new List<FastNoiseLite>()
		{ // Noise Layers
			new FastNoiseLite(0,0.02f, 10)
		}, new List<FeatureBiomeInstance>())}
	};
	public enum BiomeType
	{
		PLAINS,
		FOREST,
		DESERT,
		MESA
	}
}

public class BlockLayer : Godot.Object
{
	public Block.BlockType block;
	public int height;

	public BlockLayer(Block.BlockType _block, int _height)
	{
		block = _block;
		height = _height;
	}
}
