namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Functions of strict offboarding forecast from threshold.</summary>
public static class StrictOffboardingForecastFromThreshold
{
    public static Dictionary<string, double> SummarizeStatementOfWorkThresholdsForForecast(List<StatementOfWorkThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<StrictOffboardingForecast> DeriveStrictOffboardingForecastsFromThreshold(List<StatementOfWorkThreshold> items)
    {
        var outValue = new List<StrictOffboardingForecast>();
        foreach (var item in items)
        {
            var derived = StrictOffboardingForecast.NewStrictOffboardingForecast(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanStrictOffboardingForecast(StrictOffboardingForecast item)
    {
        return StatementOfWorkThresholdPolicy.StatementOfWorkThresholdBucket(item.MeasuredValue());
    }
}
