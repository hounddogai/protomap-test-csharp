namespace Acme.Workforce.Contractors.Offboarding.Profile;

using System.Globalization;

/// <summary>How screens and exports show a offboarding policy.</summary>
public sealed record OffboardingPolicyView(string Code, string Title, string Maximum, string Ratio, string EndsAt, string Team, string StatusLabel)
{
    public static OffboardingPolicyView ToOffboardingPolicyView(OffboardingPolicy item)
    {
        return new OffboardingPolicyView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingPolicyView> ToOffboardingPolicyViews(List<OffboardingPolicy> items)
    {
        var views = new List<OffboardingPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingPolicyView(item));
        }
        return views;
    }

    public static double SumOffboardingPolicyView(OffboardingPolicyView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedRatio + (double)parsedEndsAt;
    }
}
