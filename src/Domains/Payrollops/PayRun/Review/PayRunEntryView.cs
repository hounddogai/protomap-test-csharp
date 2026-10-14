namespace Acme.Workforce.Payrollops.PayRun.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run entry.</summary>
public sealed record PayRunEntryView(string Code, string Title, string Active, string Score, string FloorValue, string GraceDays, string StatusLabel)
{
    public static PayRunEntryView ToPayRunEntryView(PayRunEntry item)
    {
        return new PayRunEntryView(
            Text.Truncate(item.Code, 64),
            item.Title,
            $"{item.Active}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunEntryView> ToPayRunEntryViews(List<PayRunEntry> items)
    {
        var views = new List<PayRunEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunEntryView(item));
        }
        return views;
    }

    public static double SumPayRunEntryView(PayRunEntryView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedScore + parsedFloorValue + (double)parsedGraceDays;
    }
}
