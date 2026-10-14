namespace Acme.Workforce.Talent.CareerPath.Agreement;

using System.Globalization;

/// <summary>How screens and exports show a career path manifest.</summary>
public sealed record CareerPathManifestView(string Code, string Title, string Amount, string Ratio, string Phone, string StatusLabel)
{
    public static CareerPathManifestView ToCareerPathManifestView(CareerPathManifest item)
    {
        return new CareerPathManifestView(
            item.Code,
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathManifestView> ToCareerPathManifestViews(List<CareerPathManifest> items)
    {
        var views = new List<CareerPathManifestView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathManifestView(item));
        }
        return views;
    }

    public static double SumCareerPathManifestView(CareerPathManifestView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedAmount + parsedRatio;
    }
}
