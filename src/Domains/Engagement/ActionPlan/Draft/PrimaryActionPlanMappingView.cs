namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary action plan mapping.</summary>
public sealed record PrimaryActionPlanMappingView(string Code, string Title, string Fte, string Segment, string Revision, string OwnerIdText, string StatusLabel)
{
    public static PrimaryActionPlanMappingView ToPrimaryActionPlanMappingView(PrimaryActionPlanMapping item)
    {
        return new PrimaryActionPlanMappingView(
            item.Code,
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryActionPlanMappingView> ToPrimaryActionPlanMappingViews(List<PrimaryActionPlanMapping> items)
    {
        var views = new List<PrimaryActionPlanMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryActionPlanMappingView(item));
        }
        return views;
    }

    public static double SumPrimaryActionPlanMappingView(PrimaryActionPlanMappingView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedRevision;
    }
}
