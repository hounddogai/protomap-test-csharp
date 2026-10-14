namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Functions of statement of work variance from forecast.</summary>
public static class StatementOfWorkVarianceFromForecast
{
    public static Dictionary<string, double> SummarizeStrictOffboardingForecastsForVariance(List<StrictOffboardingForecast> items)
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

    public static List<StatementOfWorkVariance> DeriveStatementOfWorkVariancesFromForecast(List<StrictOffboardingForecast> items)
    {
        var outValue = new List<StatementOfWorkVariance>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkVariance.NewStatementOfWorkVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileStatementOfWorkVariance(StatementOfWorkVariance item)
    {
        return LenientOffboardingProfilePolicy.NormalizeLenientOffboardingProfileCode(item.Code);
    }
}
