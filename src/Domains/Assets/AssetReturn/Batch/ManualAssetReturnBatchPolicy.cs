namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual asset return batch policy.</summary>
public static class ManualAssetReturnBatchPolicy
{
    public static string ManualAssetReturnBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double ManualAssetReturnBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.74;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
