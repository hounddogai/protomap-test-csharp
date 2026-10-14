namespace Acme.Workforce.Assets.Warranty.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty override.</summary>
public sealed record WarrantyOverrideView(string Code, string LabelText, string PriorityText, string Location, string TitleText, string LocaleText, string StartsAt, string CostCenter, string Name, string StatusLabel)
{
    public static WarrantyOverrideView ToWarrantyOverrideView(WarrantyOverride item)
    {
        return new WarrantyOverrideView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Title,
            item.Locale,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyOverrideView> ToWarrantyOverrideViews(List<WarrantyOverride> items)
    {
        var views = new List<WarrantyOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyOverrideView(item));
        }
        return views;
    }

    public static double SumWarrantyOverrideView(WarrantyOverrideView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedStartsAt;
    }
}
