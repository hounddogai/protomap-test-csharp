namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Functions of default career path exception from manifest.</summary>
public static class DefaultCareerPathExceptionFromManifest
{
    public static Dictionary<string, double> SummarizeCappedCareerPathManifestsForException(List<CappedCareerPathManifest> items)
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

    public static List<DefaultCareerPathException> DeriveDefaultCareerPathExceptionsFromManifest(List<CappedCareerPathManifest> items)
    {
        var outValue = new List<DefaultCareerPathException>();
        foreach (var item in items)
        {
            var derived = DefaultCareerPathException.NewDefaultCareerPathException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceDefaultCareerPathException(DefaultCareerPathException item)
    {
        return CareerPathAuditPolicy.NormalizeCareerPathAuditCode(item.Code);
    }
}
