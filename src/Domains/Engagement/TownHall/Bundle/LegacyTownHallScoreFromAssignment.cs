namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Functions of legacy town hall score from assignment.</summary>
public static class LegacyTownHallScoreFromAssignment
{
    public static Dictionary<string, double> SummarizeLegacyTownHallAssignmentsForScore(List<LegacyTownHallAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<LegacyTownHallScore> DeriveLegacyTownHallScoresFromAssignment(List<LegacyTownHallAssignment> items)
    {
        var outValue = new List<LegacyTownHallScore>();
        foreach (var item in items)
        {
            var derived = LegacyTownHallScore.NewLegacyTownHallScore(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLegacyTownHallScoreExtra(LegacyTownHallScore item)
    {
        return BatchedTownHallRequestPolicy.NormalizeBatchedTownHallRequestCode(item.Code);
    }
}
