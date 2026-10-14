namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient laptop manifest.</summary>
public sealed record LenientLaptopManifestView(string CodeText, string NameText, string Score, string ManagerIdText, string PhoneText, string Rate, string Attempts, string WindowDaysText, string Threshold, string Team, string StatusLabel)
{
    public static LenientLaptopManifestView ToLenientLaptopManifestView(LenientLaptopManifest item)
    {
        return new LenientLaptopManifestView(
            item.Code,
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Phone,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientLaptopManifestView> ToLenientLaptopManifestViews(List<LenientLaptopManifest> items)
    {
        var views = new List<LenientLaptopManifestView>();
        foreach (var item in items)
        {
            views.Add(ToLenientLaptopManifestView(item));
        }
        return views;
    }

    public static double SumLenientLaptopManifestView(LenientLaptopManifestView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedScore + parsedRate + (double)parsedAttempts;
    }
}
