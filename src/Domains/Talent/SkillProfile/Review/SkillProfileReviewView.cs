namespace Acme.Workforce.Talent.SkillProfile.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile review.</summary>
public sealed record SkillProfileReviewView(string CodeText, string Name, string WindowDaysText, string Reviewer, string Minimum, string Attempts, string Title, string LimitText, string Amount, string NotesText, string Department, string StatusLabel)
{
    public static SkillProfileReviewView ToSkillProfileReviewView(SkillProfileReview item)
    {
        return new SkillProfileReviewView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 40),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileReviewView> ToSkillProfileReviewViews(List<SkillProfileReview> items)
    {
        var views = new List<SkillProfileReviewView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileReviewView(item));
        }
        return views;
    }

    public static double SumSkillProfileReviewView(SkillProfileReviewView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedMinimum + (double)parsedAttempts;
    }
}
