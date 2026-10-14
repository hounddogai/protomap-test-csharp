namespace Acme.Workforce.Absence.LeaveRequest.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict leave request review.</summary>
public sealed record StrictLeaveRequestReviewView(string Code, string Title, string CategoryText, string Grade, string CostCenter, string NameText, string AmountText, string StatusLabel)
{
    public static StrictLeaveRequestReviewView ToStrictLeaveRequestReviewView(StrictLeaveRequestReview item)
    {
        return new StrictLeaveRequestReviewView(
            item.Code,
            item.Title,
            Text.Truncate(item.Category, 120),
            Text.Truncate(item.Grade, 40),
            item.CostCenter,
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictLeaveRequestReviewView> ToStrictLeaveRequestReviewViews(List<StrictLeaveRequestReview> items)
    {
        var views = new List<StrictLeaveRequestReviewView>();
        foreach (var item in items)
        {
            views.Add(ToStrictLeaveRequestReviewView(item));
        }
        return views;
    }

    public static double SumStrictLeaveRequestReviewView(StrictLeaveRequestReviewView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
