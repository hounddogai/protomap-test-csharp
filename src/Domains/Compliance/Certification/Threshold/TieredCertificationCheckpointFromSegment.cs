namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Functions of tiered certification checkpoint from segment.</summary>
public static class TieredCertificationCheckpointFromSegment
{
    public static Dictionary<string, double> SummarizeTrainingDueSegmentsForCheckpoint(List<TrainingDueSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<TieredCertificationCheckpoint> DeriveTieredCertificationCheckpointsFromSegment(List<TrainingDueSegment> items)
    {
        var outValue = new List<TieredCertificationCheckpoint>();
        foreach (var item in items)
        {
            var derived = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreTieredCertificationCheckpoint(TieredCertificationCheckpoint item)
    {
        return TrainingDueSegmentPolicy.NormalizeTrainingDueSegmentCode(item.Code);
    }
}
