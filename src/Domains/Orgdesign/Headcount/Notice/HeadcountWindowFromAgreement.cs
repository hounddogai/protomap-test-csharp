namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Functions of headcount window from agreement.</summary>
public static class HeadcountWindowFromAgreement
{
    public static Dictionary<string, double> SummarizeHeadcountAgreementsForWindow(List<HeadcountAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<HeadcountWindow> DeriveHeadcountWindowsFromAgreement(List<HeadcountAgreement> items)
    {
        var outValue = new List<HeadcountWindow>();
        foreach (var item in items)
        {
            var derived = HeadcountWindow.NewHeadcountWindow(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileHeadcountWindow(HeadcountWindow item)
    {
        return HeadcountAgreementPolicy.ClassifyHeadcountAgreement(item.EffectiveLoad());
    }
}
