using System.Numerics;

namespace Combinations.Models.Internal;

public class CombinationState
{
    public bool IsStarted { get; set; }
    public int N { get; set; }
    public BigInteger Total { get; set; }
    public BigInteger NextIndex { get; set; }
    public BigInteger? LastDisplayedIndex { get; set; }
    public BigInteger? AllModeAnchor { get; set; }
    public int[]? LastPermutation { get; set; }

    public void Clear()
    {
        IsStarted = false;
        N = 0;
        Total = BigInteger.Zero;
        NextIndex = BigInteger.Zero;
        LastDisplayedIndex = null;
        AllModeAnchor = null;
        LastPermutation = null;
    }
}
