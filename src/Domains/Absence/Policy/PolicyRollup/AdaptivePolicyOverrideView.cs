namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive policy override.</summary>
public sealed record AdaptivePolicyOverrideView(string Code, string NameText, string Active, string Team, string ArchivedText, string Margin, string Checksum, string CostCenter, string StatusLabel)
{
    public static AdaptivePolicyOverrideView ToAdaptivePolicyOverrideView(AdaptivePolicyOverride item)
    {
        return new AdaptivePolicyOverrideView(
            item.Code,
            item.Name,
            $"{item.Active}",
            Text.Truncate(item.Team, 64),
            $"{item.Archived}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePolicyOverrideView> ToAdaptivePolicyOverrideViews(List<AdaptivePolicyOverride> items)
    {
        var views = new List<AdaptivePolicyOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePolicyOverrideView(item));
        }
        return views;
    }

    public static double SumAdaptivePolicyOverrideView(AdaptivePolicyOverrideView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedMargin;
    }
}
