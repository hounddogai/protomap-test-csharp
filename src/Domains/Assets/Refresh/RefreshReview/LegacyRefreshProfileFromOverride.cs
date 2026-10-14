namespace Acme.Workforce.Assets.Refresh.RefreshReview;

using Acme.Workforce.Assets.Assignment.Snapshot;

/// <summary>Functions of legacy refresh profile from override.</summary>
public static class LegacyRefreshProfileFromOverride
{
    public static Dictionary<string, double> SummarizeAssignmentOverridesForProfile(List<AssignmentOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<LegacyRefreshProfile> DeriveLegacyRefreshProfilesFromOverride(List<AssignmentOverride> items)
    {
        var outValue = new List<LegacyRefreshProfile>();
        foreach (var item in items)
        {
            var derived = LegacyRefreshProfile.NewLegacyRefreshProfile(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLegacyRefreshProfile(LegacyRefreshProfile item)
    {
        return DefaultAssignmentSnapshotPolicy.ClassifyDefaultAssignmentSnapshot(item.WeightedLoad());
    }
}
