namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor adjustment.</summary>
public sealed record VisitorAdjustmentView(string CodeText, string TitleText, string AttemptsText, string WindowDays, string ScoreText, string QuantityText, string RegionText, string LeaveDaysText, string Channel, string Country, string StatusLabel)
{
    public static VisitorAdjustmentView ToVisitorAdjustmentView(VisitorAdjustment item)
    {
        return new VisitorAdjustmentView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorAdjustmentView> ToVisitorAdjustmentViews(List<VisitorAdjustment> items)
    {
        var views = new List<VisitorAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorAdjustmentView(item));
        }
        return views;
    }

    public static double SumVisitorAdjustmentView(VisitorAdjustmentView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedWindowDays + parsedScore;
    }
}
