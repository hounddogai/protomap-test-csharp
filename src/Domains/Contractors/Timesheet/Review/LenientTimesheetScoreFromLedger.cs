namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>Functions of lenient timesheet score from ledger.</summary>
public static class LenientTimesheetScoreFromLedger
{
    public static Dictionary<string, double> SummarizeStatementOfWorkLedgersForScore(List<StatementOfWorkLedger> items)
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

    public static List<LenientTimesheetScore> DeriveLenientTimesheetScoresFromLedger(List<StatementOfWorkLedger> items)
    {
        var outValue = new List<LenientTimesheetScore>();
        foreach (var item in items)
        {
            var derived = LenientTimesheetScore.NewLenientTimesheetScore(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveLenientTimesheetScore(LenientTimesheetScore item)
    {
        return RegionalStatementOfWorkDigestPolicy.RegionalStatementOfWorkDigestWeight(7, true);
    }
}
