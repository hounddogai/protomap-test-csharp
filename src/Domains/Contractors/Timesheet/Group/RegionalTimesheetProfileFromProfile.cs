namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Contractors.StatementOfWork.Profile;

/// <summary>Functions of regional timesheet profile from profile.</summary>
public static class RegionalTimesheetProfileFromProfile
{
    public static Dictionary<string, double> SummarizeManualStatementOfWorkProfilesForProfile(List<ManualStatementOfWorkProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<RegionalTimesheetProfile> DeriveRegionalTimesheetProfilesFromProfile(List<ManualStatementOfWorkProfile> items)
    {
        var outValue = new List<RegionalTimesheetProfile>();
        foreach (var item in items)
        {
            var derived = RegionalTimesheetProfile.NewRegionalTimesheetProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleRegionalTimesheetProfile(RegionalTimesheetProfile item)
    {
        return LenientStatementOfWorkRequestPolicy.NormalizeLenientStatementOfWorkRequestCode(item.Code);
    }
}
