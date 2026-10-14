namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using Acme.Workforce.Assets.Assignment.Request;

/// <summary>Functions of refresh template from item.</summary>
public static class RefreshTemplateFromItem
{
    public static Dictionary<string, double> SummarizeCappedAssignmentItemsForTemplate(List<CappedAssignmentItem> items)
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

    public static List<RefreshTemplate> DeriveRefreshTemplatesFromItem(List<CappedAssignmentItem> items)
    {
        var outValue = new List<RefreshTemplate>();
        foreach (var item in items)
        {
            var derived = RefreshTemplate.NewRefreshTemplate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectRefreshTemplate(RefreshTemplate item)
    {
        return AssignmentRequestPolicy.NormalizeAssignmentRequestCode(item.Code);
    }
}
