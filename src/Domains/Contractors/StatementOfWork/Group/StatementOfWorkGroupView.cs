namespace Acme.Workforce.Contractors.StatementOfWork.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work group.</summary>
public sealed record StatementOfWorkGroupView(string Code, string Label, string Minimum, string GraceDays, string Unit, string NameText, string Grade, string Title, string Approved, string Score, string ExternalRef, string StatusLabel)
{
    public static StatementOfWorkGroupView ToStatementOfWorkGroupView(StatementOfWorkGroup item)
    {
        return new StatementOfWorkGroupView(
            item.Code,
            item.Label,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 120),
            item.Name,
            item.Grade,
            item.Title,
            $"{item.Approved}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkGroupView> ToStatementOfWorkGroupViews(List<StatementOfWorkGroup> items)
    {
        var views = new List<StatementOfWorkGroupView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkGroupView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkGroupView(StatementOfWorkGroupView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedGraceDays + parsedScore;
    }
}
