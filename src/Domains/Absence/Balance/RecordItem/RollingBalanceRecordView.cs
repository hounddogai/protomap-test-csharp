namespace Acme.Workforce.Absence.Balance.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling balance record.</summary>
public sealed record RollingBalanceRecordView(string Code, string Label, string SourceText, string UpdatedAtText, string Summary, string Sequence, string Category, string Locale, string StatusLabel)
{
    public static RollingBalanceRecordView ToRollingBalanceRecordView(RollingBalanceRecord item)
    {
        return new RollingBalanceRecordView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Source,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 40),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.Locale, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingBalanceRecordView> ToRollingBalanceRecordViews(List<RollingBalanceRecord> items)
    {
        var views = new List<RollingBalanceRecordView>();
        foreach (var item in items)
        {
            views.Add(ToRollingBalanceRecordView(item));
        }
        return views;
    }

    public static double SumRollingBalanceRecordView(RollingBalanceRecordView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedSequence;
    }
}
