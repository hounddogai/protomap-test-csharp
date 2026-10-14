namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary visitor override.</summary>
public sealed record PrimaryVisitorOverrideView(string Code, string Name, string DurationMinutes, string Category, string Minimum, string CreatedAt, string OwnerId, string Badge, string Revision, string StatusLabel)
{
    public static PrimaryVisitorOverrideView ToPrimaryVisitorOverrideView(PrimaryVisitorOverride item)
    {
        return new PrimaryVisitorOverrideView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 64),
            Text.Truncate(item.Badge, 120),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryVisitorOverrideView> ToPrimaryVisitorOverrideViews(List<PrimaryVisitorOverride> items)
    {
        var views = new List<PrimaryVisitorOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryVisitorOverrideView(item));
        }
        return views;
    }

    public static double SumPrimaryVisitorOverrideView(PrimaryVisitorOverrideView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedMinimum + (double)parsedCreatedAt;
    }
}
