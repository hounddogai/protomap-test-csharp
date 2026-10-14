namespace Acme.Workforce.Contractors.Offboarding.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding profile.</summary>
public sealed record OffboardingProfileView(string CodeText, string Label, string CurrencyText, string ManagerId, string Version, string CreatedAt, string StatusLabel)
{
    public static OffboardingProfileView ToOffboardingProfileView(OffboardingProfile item)
    {
        return new OffboardingProfileView(
            item.Code,
            item.Label,
            item.Currency,
            Text.Truncate(item.ManagerId, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingProfileView> ToOffboardingProfileViews(List<OffboardingProfile> items)
    {
        var views = new List<OffboardingProfileView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingProfileView(item));
        }
        return views;
    }

    public static double SumOffboardingProfileView(OffboardingProfileView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedCreatedAt;
    }
}
