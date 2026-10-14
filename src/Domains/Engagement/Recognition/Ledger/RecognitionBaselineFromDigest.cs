namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Functions of recognition baseline from digest.</summary>
public static class RecognitionBaselineFromDigest
{
    public static Dictionary<string, double> SummarizeFallbackRecognitionDigestsForBaseline(List<FallbackRecognitionDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<RecognitionBaseline> DeriveRecognitionBaselinesFromDigest(List<FallbackRecognitionDigest> items)
    {
        var outValue = new List<RecognitionBaseline>();
        foreach (var item in items)
        {
            var derived = RecognitionBaseline.NewRecognitionBaseline(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeRecognitionBaseline(RecognitionBaseline item)
    {
        return ManualRecognitionSegmentPolicy.ManualRecognitionSegmentWeight(6, true);
    }
}
