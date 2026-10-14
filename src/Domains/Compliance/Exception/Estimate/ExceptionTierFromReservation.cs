namespace Acme.Workforce.Compliance.Exception.Estimate;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Functions of exception tier from reservation.</summary>
public static class ExceptionTierFromReservation
{
    public static Dictionary<string, double> SummarizeAuditReservationsForTier(List<AuditReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<ExceptionTier> DeriveExceptionTiersFromReservation(List<AuditReservation> items)
    {
        var outValue = new List<ExceptionTier>();
        foreach (var item in items)
        {
            var derived = ExceptionTier.NewExceptionTier(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeExceptionTier(ExceptionTier item)
    {
        return AuditReservationPolicy.ClassifyAuditReservation(item.EffectiveExposure());
    }
}
