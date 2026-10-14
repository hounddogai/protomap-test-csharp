namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work record.</summary>
public sealed record StatementOfWorkRecordView(string Code, string Name, string CategoryText, string Hours, string Active, string PhoneText, string TenureMonths, string Skill, string NotesText, string Grade, string Priority, string StatusLabel)
{
    public static StatementOfWorkRecordView ToStatementOfWorkRecordView(StatementOfWorkRecord item)
    {
        return new StatementOfWorkRecordView(
            item.Code,
            item.Name,
            item.Category,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Phone,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 64),
            Text.Truncate(item.Notes, 64),
            item.Grade,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkRecordView> ToStatementOfWorkRecordViews(List<StatementOfWorkRecord> items)
    {
        var views = new List<StatementOfWorkRecordView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkRecordView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkRecordView(StatementOfWorkRecordView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedTenureMonths + (double)parsedPriority;
    }
}
