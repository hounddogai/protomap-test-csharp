namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal pulse survey quota.</summary>
public sealed record SeasonalPulseSurveyQuotaView(string Code, string Label, string StartsAt, string MaximumText, string ManagerId, string Revision, string ExternalRef, string ChannelText, string ReviewerText, string Pinned, string StatusLabel)
{
    public static SeasonalPulseSurveyQuotaView ToSeasonalPulseSurveyQuotaView(SeasonalPulseSurveyQuota item)
    {
        return new SeasonalPulseSurveyQuotaView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 120),
            item.Channel,
            item.Reviewer,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPulseSurveyQuotaView> ToSeasonalPulseSurveyQuotaViews(List<SeasonalPulseSurveyQuota> items)
    {
        var views = new List<SeasonalPulseSurveyQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPulseSurveyQuotaView(item));
        }
        return views;
    }

    public static double SumSeasonalPulseSurveyQuotaView(SeasonalPulseSurveyQuotaView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedMaximum + (double)parsedRevision;
    }
}
