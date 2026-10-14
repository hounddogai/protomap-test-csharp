namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled warranty review.</summary>
public sealed record ScheduledWarrantyReviewView(string Code, string Title, string LeaveDays, string Grade, string VisibleText, string MarginText, string Priority, string StatusLabel)
{
    public static ScheduledWarrantyReviewView ToScheduledWarrantyReviewView(ScheduledWarrantyReview item)
    {
        return new ScheduledWarrantyReviewView(
            item.Code,
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 120),
            $"{item.Visible}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledWarrantyReviewView> ToScheduledWarrantyReviewViews(List<ScheduledWarrantyReview> items)
    {
        var views = new List<ScheduledWarrantyReviewView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledWarrantyReviewView(item));
        }
        return views;
    }

    public static double SumScheduledWarrantyReviewView(ScheduledWarrantyReviewView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedMargin + (double)parsedPriority;
    }
}
