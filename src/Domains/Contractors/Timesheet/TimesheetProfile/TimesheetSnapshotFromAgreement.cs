namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Functions of timesheet snapshot from agreement.</summary>
public static class TimesheetSnapshotFromAgreement
{
    public static Dictionary<string, double> SummarizeOffboardingAgreementsForSnapshot(List<OffboardingAgreement> items)
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

    public static List<TimesheetSnapshot> DeriveTimesheetSnapshotsFromAgreement(List<OffboardingAgreement> items)
    {
        var outValue = new List<TimesheetSnapshot>();
        foreach (var item in items)
        {
            var derived = TimesheetSnapshot.NewTimesheetSnapshot(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitTimesheetSnapshot(TimesheetSnapshot item)
    {
        return GlobalOffboardingRequestPolicy.GlobalOffboardingRequestBucket(item.ProjectedValue());
    }
}
