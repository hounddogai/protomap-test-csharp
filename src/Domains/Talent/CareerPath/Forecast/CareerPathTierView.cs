namespace Acme.Workforce.Talent.CareerPath.Forecast;

using System.Globalization;

/// <summary>How screens and exports show a career path tier.</summary>
public sealed record CareerPathTierView(string Code, string Name, string VersionText, string ArchivedText, string Locale, string Maximum, string GraceDays, string OwnerIdText, string StatusLabel)
{
    public static CareerPathTierView ToCareerPathTierView(CareerPathTier item)
    {
        return new CareerPathTierView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Locale,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathTierView> ToCareerPathTierViews(List<CareerPathTier> items)
    {
        var views = new List<CareerPathTierView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathTierView(item));
        }
        return views;
    }

    public static double SumCareerPathTierView(CareerPathTierView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedMaximum + (double)parsedGraceDays;
    }
}
