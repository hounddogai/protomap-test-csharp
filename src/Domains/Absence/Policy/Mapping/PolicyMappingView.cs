namespace Acme.Workforce.Absence.Policy.Mapping;

using System.Globalization;

/// <summary>How screens and exports show a policy mapping.</summary>
public sealed record PolicyMappingView(string Code, string Name, string OwnerId, string Visible, string Unit, string Summary, string LimitText, string StatusLabel)
{
    public static PolicyMappingView ToPolicyMappingView(PolicyMapping item)
    {
        return new PolicyMappingView(
            item.Code,
            item.Name,
            item.OwnerId,
            $"{item.Visible}",
            item.Unit,
            item.Summary,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyMappingView> ToPolicyMappingViews(List<PolicyMapping> items)
    {
        var views = new List<PolicyMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyMappingView(item));
        }
        return views;
    }

    public static double SumPolicyMappingView(PolicyMappingView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
