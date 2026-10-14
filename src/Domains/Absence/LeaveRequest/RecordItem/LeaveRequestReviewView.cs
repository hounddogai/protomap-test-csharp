namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request review.</summary>
public sealed record LeaveRequestReviewView(string Code, string LabelText, string Budget, string Channel, string HoursText, string Rating, string Fte, string Archived, string Department, string StartsAt, string Category, string StatusLabel)
{
    public static LeaveRequestReviewView ToLeaveRequestReviewView(LeaveRequestReview item)
    {
        return new LeaveRequestReviewView(
            item.Code,
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Department,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestReviewView> ToLeaveRequestReviewViews(List<LeaveRequestReview> items)
    {
        var views = new List<LeaveRequestReviewView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestReviewView(item));
        }
        return views;
    }

    public static double SumLeaveRequestReviewView(LeaveRequestReviewView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedBudget + parsedHours + parsedRating;
    }
}
