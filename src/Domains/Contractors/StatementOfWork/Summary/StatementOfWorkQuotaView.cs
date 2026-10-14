namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work quota.</summary>
public sealed record StatementOfWorkQuotaView(string Code, string Label, string SourceText, string ContactEmailText, string Archived, string SalaryBand, string Region, string ReviewerText, string Score, string StatusLabel)
{
    public static StatementOfWorkQuotaView ToStatementOfWorkQuotaView(StatementOfWorkQuota item)
    {
        return new StatementOfWorkQuotaView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Source,
            Text.Truncate(item.ContactEmail, 120),
            $"{item.Archived}",
            item.SalaryBand,
            item.Region,
            item.Reviewer,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkQuotaView> ToStatementOfWorkQuotaViews(List<StatementOfWorkQuota> items)
    {
        var views = new List<StatementOfWorkQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkQuotaView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkQuotaView(StatementOfWorkQuotaView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
