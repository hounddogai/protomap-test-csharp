namespace Acme.Workforce.Absence.Balance.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance record.</summary>
public sealed record BalanceRecordView(string Code, string Name, string EmployeeIdText, string StartsAt, string Archived, string Channel, string DurationMinutesText, string Score, string StatusLabel)
{
    public static BalanceRecordView ToBalanceRecordView(BalanceRecord item)
    {
        return new BalanceRecordView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 80),
            item.EmployeeId,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Channel, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceRecordView> ToBalanceRecordViews(List<BalanceRecord> items)
    {
        var views = new List<BalanceRecordView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceRecordView(item));
        }
        return views;
    }

    public static double SumBalanceRecordView(BalanceRecordView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedDurationMinutes + parsedScore;
    }
}
