using Godot;
using System;
using System.Collections.Generic;
using Object = Godot.Object;

public class Feature : Object
{
	public Vector2 heightMinMax = new Vector2(4,8);
	public Block.BlockType bark;
	public Block.BlockType leaves;
	public int trunkWidth = 1;
	public int leavesWidth = 2;

	public Feature(Vector2 _heightMinMax, Block.BlockType _bark, Block.BlockType _leaves, int _trunkWidth = 1, int _leavesWidth = 2)
	{
		heightMinMax = _heightMinMax;
		bark = _bark;
		leaves = _leaves;
		trunkWidth = _trunkWidth;
		leavesWidth = _leavesWidth;
	}

	public static Dictionary<FeatureType,Feature> featureDict = new Dictionary<FeatureType,Feature>()
	{
		{FeatureType.OAK_TREE, new Feature(new Vector2(4,8), Block.BlockType.OAK_LOG, Block.BlockType.OAK_LEAVES)},
		{FeatureType.BIRCH_TREE, new Feature(new Vector2(5,8), Block.BlockType.BIRCH_LOG, Block.BlockType.BIRCH_LEAVES)},
		{FeatureType.SPRUCE_TREE, new Feature(new Vector2(6,10), Block.BlockType.SPRUCE_LOG, Block.BlockType.SPRUCE_LEAVES)},
		{FeatureType.JUNGLE_TREE, new Feature(new Vector2(8,16), Block.BlockType.JUNGLE_LOG, Block.BlockType.JUNGLE_LEAVES)},
		{FeatureType.ACACIA_TREE, new Feature(new Vector2(4,8), Block.BlockType.ACACIA_LOG, Block.BlockType.ACACIA_LEAVES)},
		{FeatureType.DARK_OAK_TREE, new Feature(new Vector2(4,7), Block.BlockType.DARK_OAK_LOG, Block.BlockType.DARK_OAK_LEAVES, 2, 3)},
		{FeatureType.CACTUS, new Feature(new Vector2(1,4), Block.BlockType.CACTUS, Block.BlockType.AIR)}
	};
	public enum FeatureType
	{
		OAK_TREE,
		BIRCH_TREE,
		SPRUCE_TREE,
		JUNGLE_TREE,
		ACACIA_TREE,
		DARK_OAK_TREE,
		CACTUS
	}
}

public class FeatureBiomeInstance : Object
{
	public Feature.FeatureType feature;
	public float probability = 0.01f;

	public FeatureBiomeInstance(Feature.FeatureType _feature, float _probability)
	{
		feature = _feature;
		probability = _probability;
	}
}
