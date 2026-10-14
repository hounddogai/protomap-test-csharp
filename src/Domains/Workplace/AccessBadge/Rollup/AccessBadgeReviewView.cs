namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge review.</summary>
public sealed record AccessBadgeReviewView(string Code, string LabelText, string GradeText, string Attempts, string LimitText, string Skill, string StatusLabel)
{
    public static AccessBadgeReviewView ToAccessBadgeReviewView(AccessBadgeReview item)
    {
        return new AccessBadgeReviewView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Grade,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeReviewView> ToAccessBadgeReviewViews(List<AccessBadgeReview> items)
    {
        var views = new List<AccessBadgeReviewView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeReviewView(item));
        }
        return views;
    }

    public static double SumAccessBadgeReviewView(AccessBadgeReviewView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedLimit;
    }
}
