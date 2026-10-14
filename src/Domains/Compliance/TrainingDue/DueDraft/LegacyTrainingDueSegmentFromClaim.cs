namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Functions of legacy training due segment from claim.</summary>
public static class LegacyTrainingDueSegmentFromClaim
{
    public static Dictionary<string, double> SummarizeManualCertificationClaimsForSegment(List<ManualCertificationClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<LegacyTrainingDueSegment> DeriveLegacyTrainingDueSegmentsFromClaim(List<ManualCertificationClaim> items)
    {
        var outValue = new List<LegacyTrainingDueSegment>();
        foreach (var item in items)
        {
            var derived = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleLegacyTrainingDueSegment(LegacyTrainingDueSegment item)
    {
        return SeasonalCertificationDigestPolicy.SeasonalCertificationDigestWeight(2, true);
    }
}
