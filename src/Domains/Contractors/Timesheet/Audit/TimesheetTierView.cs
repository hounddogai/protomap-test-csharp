namespace Acme.Workforce.Contractors.Timesheet.Audit;

using System.Globalization;

/// <summary>How screens and exports show a timesheet tier.</summary>
public sealed record TimesheetTierView(string CodeText, string Title, string Retries, string CostCenter, string Ratio, string Weight, string ChecksumText, string SourceText, string CurrencyText, string StatusLabel)
{
    public static TimesheetTierView ToTimesheetTierView(TimesheetTier item)
    {
        return new TimesheetTierView(
            item.Code,
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.Source,
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetTierView> ToTimesheetTierViews(List<TimesheetTier> items)
    {
        var views = new List<TimesheetTierView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetTierView(item));
        }
        return views;
    }

    public static double SumTimesheetTierView(TimesheetTierView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedRatio + parsedWeight;
    }
}
