namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work draft.</summary>
public sealed record StatementOfWorkDraftView(string CodeText, string Label, string GraceDays, string ApprovedText, string Score, string Pinned, string Segment, string Ratio, string Name, string CostCenter, string StatusLabel)
{
    public static StatementOfWorkDraftView ToStatementOfWorkDraftView(StatementOfWorkDraft item)
    {
        return new StatementOfWorkDraftView(
            item.Code,
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Segment, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkDraftView> ToStatementOfWorkDraftViews(List<StatementOfWorkDraft> items)
    {
        var views = new List<StatementOfWorkDraftView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkDraftView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkDraftView(StatementOfWorkDraftView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedScore + parsedRatio;
    }
}
