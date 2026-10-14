namespace Acme.Workforce.Contractors.Offboarding.Draft;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Functions of offboarding draft from window.</summary>
public static class OffboardingDraftFromWindow
{
    public static Dictionary<string, double> SummarizeTimesheetWindowsForDraft(List<TimesheetWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<OffboardingDraft> DeriveOffboardingDraftsFromWindow(List<TimesheetWindow> items)
    {
        var outValue = new List<OffboardingDraft>();
        foreach (var item in items)
        {
            var derived = OffboardingDraft.NewOffboardingDraft(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveOffboardingDraft(OffboardingDraft item)
    {
        return LenientTimesheetPreferencePolicy.ClassifyLenientTimesheetPreference(item.WeightedLoad());
    }
}
