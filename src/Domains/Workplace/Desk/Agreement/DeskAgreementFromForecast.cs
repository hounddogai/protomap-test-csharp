namespace Acme.Workforce.Workplace.Desk.Agreement;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Functions of desk agreement from forecast.</summary>
public static class DeskAgreementFromForecast
{
    public static Dictionary<string, double> SummarizeDeskForecastsForAgreement(List<DeskForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<DeskAgreement> DeriveDeskAgreementsFromForecast(List<DeskForecast> items)
    {
        var outValue = new List<DeskAgreement>();
        foreach (var item in items)
        {
            var derived = DeskAgreement.NewDeskAgreement(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveDeskAgreement(DeskAgreement item)
    {
        return DeskSummaryPolicy.DeskSummaryWeight(7, true);
    }
}
