namespace Acme.Workforce.Contractors.Timesheet.Review;

using System.Globalization;

/// <summary>How screens and exports show a timesheet review.</summary>
public sealed record TimesheetReviewView(string Code, string Label, string Locale, string EmployeeId, string ApprovedText, string MinimumText, string JobCodeText, string ExpiresAtText, string Ratio, string Quantity, string Margin, string StatusLabel)
{
    public static TimesheetReviewView ToTimesheetReviewView(TimesheetReview item)
    {
        return new TimesheetReviewView(
            item.Code,
            item.Label,
            item.Locale,
            item.EmployeeId,
            $"{item.Approved}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetReviewView> ToTimesheetReviewViews(List<TimesheetReview> items)
    {
        var views = new List<TimesheetReviewView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetReviewView(item));
        }
        return views;
    }

    public static double SumTimesheetReviewView(TimesheetReviewView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedExpiresAt + parsedRatio;
    }
}
