namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey review.</summary>
public sealed record PulseSurveyReviewView(string Code, string Label, string SkillText, string Summary, string Revision, string Notes, string CostCenter, string Priority, string Locale, string SourceText, string Rate, string StatusLabel)
{
    public static PulseSurveyReviewView ToPulseSurveyReviewView(PulseSurveyReview item)
    {
        return new PulseSurveyReviewView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Skill, 64),
            item.Summary,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.CostCenter, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.Source,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyReviewView> ToPulseSurveyReviewViews(List<PulseSurveyReview> items)
    {
        var views = new List<PulseSurveyReviewView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyReviewView(item));
        }
        return views;
    }

    public static double SumPulseSurveyReviewView(PulseSurveyReviewView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedPriority + parsedRate;
    }
}
