namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using System.Globalization;

/// <summary>How screens and exports show a global policy allocation.</summary>
public sealed record GlobalPolicyAllocationView(string CodeText, string Name, string Reviewer, string Amount, string RetriesText, string OwnerId, string StatusLabel)
{
    public static GlobalPolicyAllocationView ToGlobalPolicyAllocationView(GlobalPolicyAllocation item)
    {
        return new GlobalPolicyAllocationView(
            item.Code,
            item.Name,
            item.Reviewer,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPolicyAllocationView> ToGlobalPolicyAllocationViews(List<GlobalPolicyAllocation> items)
    {
        var views = new List<GlobalPolicyAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPolicyAllocationView(item));
        }
        return views;
    }

    public static double SumGlobalPolicyAllocationView(GlobalPolicyAllocationView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedRetries;
    }
}
