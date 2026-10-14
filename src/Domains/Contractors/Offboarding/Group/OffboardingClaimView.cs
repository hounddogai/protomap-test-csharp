namespace Acme.Workforce.Contractors.Offboarding.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding claim.</summary>
public sealed record OffboardingClaimView(string CodeText, string NameText, string Phone, string ExternalRefText, string LocationText, string ScoreText, string StatusLabel)
{
    public static OffboardingClaimView ToOffboardingClaimView(OffboardingClaim item)
    {
        return new OffboardingClaimView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Phone,
            Text.Truncate(item.ExternalRef, 80),
            item.Location,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingClaimView> ToOffboardingClaimViews(List<OffboardingClaim> items)
    {
        var views = new List<OffboardingClaimView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingClaimView(item));
        }
        return views;
    }

    public static double SumOffboardingClaimView(OffboardingClaimView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
