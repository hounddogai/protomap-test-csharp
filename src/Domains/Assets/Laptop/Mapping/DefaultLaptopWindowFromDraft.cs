namespace Acme.Workforce.Assets.Laptop.Mapping;

using Acme.Workforce.Assets.Assignment.Rollup;

/// <summary>Functions of default laptop window from draft.</summary>
public static class DefaultLaptopWindowFromDraft
{
    public static Dictionary<string, double> SummarizeAssignmentDraftsForWindow(List<AssignmentDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<DefaultLaptopWindow> DeriveDefaultLaptopWindowsFromDraft(List<AssignmentDraft> items)
    {
        var outValue = new List<DefaultLaptopWindow>();
        foreach (var item in items)
        {
            var derived = DefaultLaptopWindow.NewDefaultLaptopWindow(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeDefaultLaptopWindow(DefaultLaptopWindow item)
    {
        return AssignmentRollupPolicy.AssignmentRollupBucket(item.MeasuredValue());
    }
}
