namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding manifest.</summary>
public sealed record OffboardingManifestView(string CodeText, string Title, string Maximum, string Unit, string ExpiresAt, string StatusLabel)
{
    public static OffboardingManifestView ToOffboardingManifestView(OffboardingManifest item)
    {
        return new OffboardingManifestView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 64),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingManifestView> ToOffboardingManifestViews(List<OffboardingManifest> items)
    {
        var views = new List<OffboardingManifestView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingManifestView(item));
        }
        return views;
    }

    public static double SumOffboardingManifestView(OffboardingManifestView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedExpiresAt;
    }
}
