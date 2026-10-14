namespace Acme.Workforce.Engagement.Feedback.Rule;

using System.Globalization;

/// <summary>How screens and exports show a legacy feedback claim.</summary>
public sealed record LegacyFeedbackClaimView(string Code, string Label, string EndsAt, string Location, string Country, string SourceText, string CreatedAt, string Description, string ExternalRefText, string Cost, string StatusLabel)
{
    public static LegacyFeedbackClaimView ToLegacyFeedbackClaimView(LegacyFeedbackClaim item)
    {
        return new LegacyFeedbackClaimView(
            item.Code,
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Country,
            item.Source,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.ExternalRef,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyFeedbackClaimView> ToLegacyFeedbackClaimViews(List<LegacyFeedbackClaim> items)
    {
        var views = new List<LegacyFeedbackClaimView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyFeedbackClaimView(item));
        }
        return views;
    }

    public static double SumLegacyFeedbackClaimView(LegacyFeedbackClaimView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedCreatedAt + parsedCost;
    }
}
