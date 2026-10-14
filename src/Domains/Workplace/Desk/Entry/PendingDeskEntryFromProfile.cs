namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Functions of pending desk entry from profile.</summary>
public static class PendingDeskEntryFromProfile
{
    public static Dictionary<string, double> SummarizeLenientDeskProfilesForEntry(List<LenientDeskProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<PendingDeskEntry> DerivePendingDeskEntriesFromProfile(List<LenientDeskProfile> items)
    {
        var outValue = new List<PendingDeskEntry>();
        foreach (var item in items)
        {
            var derived = PendingDeskEntry.NewPendingDeskEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimatePendingDeskEntry(PendingDeskEntry item)
    {
        return LenientDeskProfilePolicy.LenientDeskProfileWeight(7, true);
    }
}
