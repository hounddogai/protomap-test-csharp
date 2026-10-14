namespace Acme.Workforce.Compliance.Audit.Slot;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Functions of audit score from mapping.</summary>
public static class AuditScoreFromMapping
{
    public static Dictionary<string, double> SummarizePrimaryAuditMappingsForScore(List<PrimaryAuditMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<AuditScore> DeriveAuditScoresFromMapping(List<PrimaryAuditMapping> items)
    {
        var outValue = new List<AuditScore>();
        foreach (var item in items)
        {
            var derived = AuditScore.NewAuditScore(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankAuditScore(AuditScore item)
    {
        return CappedAuditReservationPolicy.NormalizeCappedAuditReservationCode(item.Code);
    }
}
