namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy statement of work record.</summary>
public sealed record LegacyStatementOfWorkRecordView(string Code, string Name, string ExternalRef, string Archived, string UpdatedAt, string Ratio, string Department, string JobCode, string LeaveDays, string Minimum, string StatusLabel)
{
    public static LegacyStatementOfWorkRecordView ToLegacyStatementOfWorkRecordView(LegacyStatementOfWorkRecord item)
    {
        return new LegacyStatementOfWorkRecordView(
            item.Code,
            item.Name,
            item.ExternalRef,
            $"{item.Archived}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 64),
            item.JobCode,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyStatementOfWorkRecordView> ToLegacyStatementOfWorkRecordViews(List<LegacyStatementOfWorkRecord> items)
    {
        var views = new List<LegacyStatementOfWorkRecordView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyStatementOfWorkRecordView(item));
        }
        return views;
    }

    public static double SumLegacyStatementOfWorkRecordView(LegacyStatementOfWorkRecordView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRatio + (double)parsedLeaveDays;
    }
}
