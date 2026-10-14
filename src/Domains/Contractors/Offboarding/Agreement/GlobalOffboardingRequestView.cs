namespace Acme.Workforce.Contractors.Offboarding.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global offboarding request.</summary>
public sealed record GlobalOffboardingRequestView(string Code, string LabelText, string CategoryText, string ContactEmail, string TeamText, string Score, string StatusLabel)
{
    public static GlobalOffboardingRequestView ToGlobalOffboardingRequestView(GlobalOffboardingRequest item)
    {
        return new GlobalOffboardingRequestView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Category,
            Text.Truncate(item.ContactEmail, 120),
            item.Team,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalOffboardingRequestView> ToGlobalOffboardingRequestViews(List<GlobalOffboardingRequest> items)
    {
        var views = new List<GlobalOffboardingRequestView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalOffboardingRequestView(item));
        }
        return views;
    }

    public static double SumGlobalOffboardingRequestView(GlobalOffboardingRequestView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
