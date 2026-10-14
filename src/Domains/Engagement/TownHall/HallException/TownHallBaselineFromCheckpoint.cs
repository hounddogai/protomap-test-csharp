namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Functions of town hall baseline from checkpoint.</summary>
public static class TownHallBaselineFromCheckpoint
{
    public static Dictionary<string, double> SummarizeRecognitionCheckpointsForBaseline(List<RecognitionCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<TownHallBaseline> DeriveTownHallBaselinesFromCheckpoint(List<RecognitionCheckpoint> items)
    {
        var outValue = new List<TownHallBaseline>();
        foreach (var item in items)
        {
            var derived = TownHallBaseline.NewTownHallBaseline(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeTownHallBaseline(TownHallBaseline item)
    {
        return RecognitionAdjustmentPolicy.NormalizeRecognitionAdjustmentCode(item.Code);
    }
}
