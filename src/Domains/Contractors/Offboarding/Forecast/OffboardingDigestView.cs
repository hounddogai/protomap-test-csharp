namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using System.Globalization;

/// <summary>How screens and exports show a offboarding digest.</summary>
public sealed record OffboardingDigestView(string Code, string Title, string LeaveDays, string SummaryText, string AttemptsText, string StatusLabel)
{
    public static OffboardingDigestView ToOffboardingDigestView(OffboardingDigest item)
    {
        return new OffboardingDigestView(
            item.Code,
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingDigestView> ToOffboardingDigestViews(List<OffboardingDigest> items)
    {
        var views = new List<OffboardingDigestView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingDigestView(item));
        }
        return views;
    }

    public static double SumOffboardingDigestView(OffboardingDigestView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedAttempts;
    }
}
