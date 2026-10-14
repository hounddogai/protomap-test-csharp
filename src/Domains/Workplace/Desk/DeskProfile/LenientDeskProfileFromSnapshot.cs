namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Workplace.Occupancy.Ledger;

/// <summary>Functions of lenient desk profile from snapshot.</summary>
public static class LenientDeskProfileFromSnapshot
{
    public static Dictionary<string, double> SummarizeDefaultOccupancySnapshotsForProfile(List<DefaultOccupancySnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<LenientDeskProfile> DeriveLenientDeskProfilesFromSnapshot(List<DefaultOccupancySnapshot> items)
    {
        var outValue = new List<LenientDeskProfile>();
        foreach (var item in items)
        {
            var derived = LenientDeskProfile.NewLenientDeskProfile(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreLenientDeskProfile(LenientDeskProfile item)
    {
        return OccupancyCyclePolicy.NormalizeOccupancyCycleCode(item.Code);
    }
}
