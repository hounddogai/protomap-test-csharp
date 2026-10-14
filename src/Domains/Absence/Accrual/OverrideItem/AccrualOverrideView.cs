namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual override.</summary>
public sealed record AccrualOverrideView(string Code, string Title, string JobCodeText, string WindowDays, string Description, string Weight, string Active, string VisibleText, string BatchSize, string StatusLabel)
{
    public static AccrualOverrideView ToAccrualOverrideView(AccrualOverride item)
    {
        return new AccrualOverrideView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.JobCode,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Visible}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualOverrideView> ToAccrualOverrideViews(List<AccrualOverride> items)
    {
        var views = new List<AccrualOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualOverrideView(item));
        }
        return views;
    }

    public static double SumAccrualOverrideView(AccrualOverrideView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedWeight + (double)parsedBatchSize;
    }
}
