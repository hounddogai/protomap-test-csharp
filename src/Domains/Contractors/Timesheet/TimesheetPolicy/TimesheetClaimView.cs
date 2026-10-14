namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using System.Globalization;

/// <summary>How screens and exports show a timesheet claim.</summary>
public sealed record TimesheetClaimView(string CodeText, string Title, string Fte, string Currency, string ManagerIdText, string Phone, string Grade, string Rating, string UpdatedAtText, string Quantity, string Skill, string StatusLabel)
{
    public static TimesheetClaimView ToTimesheetClaimView(TimesheetClaim item)
    {
        return new TimesheetClaimView(
            item.Code,
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.ManagerId,
            item.Phone,
            item.Grade,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetClaimView> ToTimesheetClaimViews(List<TimesheetClaim> items)
    {
        var views = new List<TimesheetClaimView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetClaimView(item));
        }
        return views;
    }

    public static double SumTimesheetClaimView(TimesheetClaimView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedFte + parsedRating + (double)parsedUpdatedAt;
    }
}
