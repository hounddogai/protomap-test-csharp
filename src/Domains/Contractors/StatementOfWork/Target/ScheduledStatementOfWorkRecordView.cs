namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled statement of work record.</summary>
public sealed record ScheduledStatementOfWorkRecordView(string Code, string NameText, string Margin, string Badge, string LeaveDaysText, string VersionText, string Minimum, string Summary, string CurrencyText, string StatusLabel)
{
    public static ScheduledStatementOfWorkRecordView ToScheduledStatementOfWorkRecordView(ScheduledStatementOfWorkRecord item)
    {
        return new ScheduledStatementOfWorkRecordView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            Text.Truncate(item.Currency, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledStatementOfWorkRecordView> ToScheduledStatementOfWorkRecordViews(List<ScheduledStatementOfWorkRecord> items)
    {
        var views = new List<ScheduledStatementOfWorkRecordView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledStatementOfWorkRecordView(item));
        }
        return views;
    }

    public static double SumScheduledStatementOfWorkRecordView(ScheduledStatementOfWorkRecordView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedLeaveDays + (double)parsedVersion;
    }
}
