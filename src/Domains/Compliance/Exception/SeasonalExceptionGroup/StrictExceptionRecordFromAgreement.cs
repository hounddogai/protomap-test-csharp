namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Functions of strict exception record from agreement.</summary>
public static class StrictExceptionRecordFromAgreement
{
    public static Dictionary<string, double> SummarizeLegacyExceptionAgreementsForRecord(List<LegacyExceptionAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<StrictExceptionRecord> DeriveStrictExceptionRecordsFromAgreement(List<LegacyExceptionAgreement> items)
    {
        var outValue = new List<StrictExceptionRecord>();
        foreach (var item in items)
        {
            var derived = StrictExceptionRecord.NewStrictExceptionRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankStrictExceptionRecord(StrictExceptionRecord item)
    {
        return LegacyExceptionAgreementPolicy.ClassifyLegacyExceptionAgreement(item.ProjectedVolume());
    }
}
