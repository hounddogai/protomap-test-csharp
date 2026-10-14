namespace Acme.Workforce.Absence.Accrual.Notice;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;

/// <summary>Functions of primary accrual notice from assignment.</summary>
public static class PrimaryAccrualNoticeFromAssignment
{
    public static Dictionary<string, double> SummarizeSeasonalHolidayAssignmentsForNotice(List<SeasonalHolidayAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<PrimaryAccrualNotice> DerivePrimaryAccrualNoticesFromAssignment(List<SeasonalHolidayAssignment> items)
    {
        var outValue = new List<PrimaryAccrualNotice>();
        foreach (var item in items)
        {
            var derived = PrimaryAccrualNotice.NewPrimaryAccrualNotice(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePrimaryAccrualNotice(PrimaryAccrualNotice item)
    {
        return SeasonalHolidayAssignmentPolicy.NormalizeSeasonalHolidayAssignmentCode(item.Code);
    }
}
