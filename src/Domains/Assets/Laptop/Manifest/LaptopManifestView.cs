namespace Acme.Workforce.Assets.Laptop.Manifest;

using System.Globalization;

/// <summary>How screens and exports show a laptop manifest.</summary>
public sealed record LaptopManifestView(string Code, string Title, string ExternalRef, string Budget, string LeaveDays, string TeamText, string StatusLabel)
{
    public static LaptopManifestView ToLaptopManifestView(LaptopManifest item)
    {
        return new LaptopManifestView(
            item.Code,
            item.Title,
            item.ExternalRef,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopManifestView> ToLaptopManifestViews(List<LaptopManifest> items)
    {
        var views = new List<LaptopManifestView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopManifestView(item));
        }
        return views;
    }

    public static double SumLaptopManifestView(LaptopManifestView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedLeaveDays;
    }
}
