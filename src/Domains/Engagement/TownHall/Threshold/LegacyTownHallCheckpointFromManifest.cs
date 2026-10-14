namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Functions of legacy town hall checkpoint from manifest.</summary>
public static class LegacyTownHallCheckpointFromManifest
{
    public static Dictionary<string, double> SummarizeRecognitionManifestsForCheckpoint(List<RecognitionManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<LegacyTownHallCheckpoint> DeriveLegacyTownHallCheckpointsFromManifest(List<RecognitionManifest> items)
    {
        var outValue = new List<LegacyTownHallCheckpoint>();
        foreach (var item in items)
        {
            var derived = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLegacyTownHallCheckpoint(LegacyTownHallCheckpoint item)
    {
        return CappedRecognitionRulePolicy.NormalizeCappedRecognitionRuleCode(item.Code);
    }
}
