namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Absence.ReturnPlan.Assignment;

/// <summary>Functions of capped accrual score from forecast.</summary>
public static class CappedAccrualScoreFromForecast
{
    public static Dictionary<string, double> SummarizeReturnPlanForecastsForScore(List<ReturnPlanForecast> items)
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

    public static List<CappedAccrualScore> DeriveCappedAccrualScoresFromForecast(List<ReturnPlanForecast> items)
    {
        var outValue = new List<CappedAccrualScore>();
        foreach (var item in items)
        {
            var derived = CappedAccrualScore.NewCappedAccrualScore(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankCappedAccrualScore(CappedAccrualScore item)
    {
        return ReturnPlanAssignmentPolicy.NormalizeReturnPlanAssignmentCode(item.Code);
    }
}
