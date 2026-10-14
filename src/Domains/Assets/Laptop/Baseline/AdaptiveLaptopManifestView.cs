namespace Acme.Workforce.Assets.Laptop.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive laptop manifest.</summary>
public sealed record AdaptiveLaptopManifestView(string Code, string Title, string Grade, string ExternalRefText, string FloorValue, string LocationText, string ActiveText, string SourceText, string Timezone, string EmployeeId, string LabelText, string StatusLabel)
{
    public static AdaptiveLaptopManifestView ToAdaptiveLaptopManifestView(AdaptiveLaptopManifest item)
    {
        return new AdaptiveLaptopManifestView(
            Text.Truncate(item.Code, 40),
            item.Title,
            Text.Truncate(item.Grade, 64),
            item.ExternalRef,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Active}",
            item.Source,
            item.Timezone,
            Text.Truncate(item.EmployeeId, 40),
            Text.Truncate(item.Label, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveLaptopManifestView> ToAdaptiveLaptopManifestViews(List<AdaptiveLaptopManifest> items)
    {
        var views = new List<AdaptiveLaptopManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveLaptopManifestView(item));
        }
        return views;
    }

    public static double SumAdaptiveLaptopManifestView(AdaptiveLaptopManifestView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedFloorValue;
    }
}
