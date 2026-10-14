namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using System.Globalization;

/// <summary>How screens and exports show a statement of work summary.</summary>
public sealed record StatementOfWorkSummaryView(string Code, string LabelText, string NotesText, string RegionText, string Visible, string Quantity, string StatusLabel)
{
    public static StatementOfWorkSummaryView ToStatementOfWorkSummaryView(StatementOfWorkSummary item)
    {
        return new StatementOfWorkSummaryView(
            item.Code,
            item.Label,
            item.Notes,
            item.Region,
            $"{item.Visible}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkSummaryView> ToStatementOfWorkSummaryViews(List<StatementOfWorkSummary> items)
    {
        var views = new List<StatementOfWorkSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkSummaryView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkSummaryView(StatementOfWorkSummaryView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
