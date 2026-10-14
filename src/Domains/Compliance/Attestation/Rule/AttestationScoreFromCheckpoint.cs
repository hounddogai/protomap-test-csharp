namespace Acme.Workforce.Compliance.Attestation.Rule;

using Acme.Workforce.Compliance.Exception.Review;

/// <summary>Functions of attestation score from checkpoint.</summary>
public static class AttestationScoreFromCheckpoint
{
    public static Dictionary<string, double> SummarizeSeasonalExceptionCheckpointsForScore(List<SeasonalExceptionCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<AttestationScore> DeriveAttestationScoresFromCheckpoint(List<SeasonalExceptionCheckpoint> items)
    {
        var outValue = new List<AttestationScore>();
        foreach (var item in items)
        {
            var derived = AttestationScore.NewAttestationScore(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveAttestationScore(AttestationScore item)
    {
        return ExceptionReviewPolicy.ExceptionReviewBucket(item.ProjectedTotal());
    }
}
