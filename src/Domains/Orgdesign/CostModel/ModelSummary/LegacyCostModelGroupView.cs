namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy cost model group.</summary>
public sealed record LegacyCostModelGroupView(string Code, string Label, string Location, string Title, string Badge, string ExpiresAtText, string FloorValue, string ContactEmail, string Team, string ReviewerText, string Name, string StatusLabel)
{
    public static LegacyCostModelGroupView ToLegacyCostModelGroupView(LegacyCostModelGroup item)
    {
        return new LegacyCostModelGroupView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Location,
            item.Title,
            item.Badge,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Team,
            item.Reviewer,
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyCostModelGroupView> ToLegacyCostModelGroupViews(List<LegacyCostModelGroup> items)
    {
        var views = new List<LegacyCostModelGroupView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyCostModelGroupView(item));
        }
        return views;
    }

    public static double SumLegacyCostModelGroupView(LegacyCostModelGroupView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedFloorValue;
    }
}
