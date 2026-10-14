namespace Acme.Workforce.Absence.Holiday.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient holiday adjustment.</summary>
public sealed record LenientHolidayAdjustmentView(string CodeText, string NameText, string Category, string Score, string Archived, string Notes, string ContactEmailText, string StatusLabel)
{
    public static LenientHolidayAdjustmentView ToLenientHolidayAdjustmentView(LenientHolidayAdjustment item)
    {
        return new LenientHolidayAdjustmentView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.Category, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Notes,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientHolidayAdjustmentView> ToLenientHolidayAdjustmentViews(List<LenientHolidayAdjustment> items)
    {
        var views = new List<LenientHolidayAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToLenientHolidayAdjustmentView(item));
        }
        return views;
    }

    public static double SumLenientHolidayAdjustmentView(LenientHolidayAdjustmentView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
