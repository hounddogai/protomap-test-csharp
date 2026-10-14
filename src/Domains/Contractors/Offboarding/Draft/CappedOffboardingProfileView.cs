namespace Acme.Workforce.Contractors.Offboarding.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped offboarding profile.</summary>
public sealed record CappedOffboardingProfileView(string Code, string TitleText, string EndsAt, string ExpiresAt, string Priority, string Ceiling, string Department, string Rate, string StatusLabel)
{
    public static CappedOffboardingProfileView ToCappedOffboardingProfileView(CappedOffboardingProfile item)
    {
        return new CappedOffboardingProfileView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedOffboardingProfileView> ToCappedOffboardingProfileViews(List<CappedOffboardingProfile> items)
    {
        var views = new List<CappedOffboardingProfileView>();
        foreach (var item in items)
        {
            views.Add(ToCappedOffboardingProfileView(item));
        }
        return views;
    }

    public static double SumCappedOffboardingProfileView(CappedOffboardingProfileView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedExpiresAt + (double)parsedPriority;
    }
}
