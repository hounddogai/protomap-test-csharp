namespace Acme.Workforce.Compensation.MeritCycle.Claim;

using Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>Functions of fallback merit cycle claim from exception.</summary>
public static class FallbackMeritCycleClaimFromException
{
    public static Dictionary<string, double> SummarizePayBandExceptionsForClaim(List<PayBandException> items)
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

    public static List<FallbackMeritCycleClaim> DeriveFallbackMeritCycleClaimsFromException(List<PayBandException> items)
    {
        var outValue = new List<FallbackMeritCycleClaim>();
        foreach (var item in items)
        {
            var derived = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyFallbackMeritCycleClaimExtra(FallbackMeritCycleClaim item)
    {
        return TieredPayBandDraftPolicy.ClassifyTieredPayBandDraft(item.MeasuredValue());
    }
}
