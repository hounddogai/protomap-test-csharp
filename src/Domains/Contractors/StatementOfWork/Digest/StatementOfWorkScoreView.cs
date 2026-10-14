namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work score.</summary>
public sealed record StatementOfWorkScoreView(string CodeText, string Name, string Unit, string NotesText, string Discount, string Limit, string Location, string Segment, string StatusLabel)
{
    public static StatementOfWorkScoreView ToStatementOfWorkScoreView(StatementOfWorkScore item)
    {
        return new StatementOfWorkScoreView(
            item.Code,
            item.Name,
            item.Unit,
            Text.Truncate(item.Notes, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 40),
            Text.Truncate(item.Segment, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkScoreView> ToStatementOfWorkScoreViews(List<StatementOfWorkScore> items)
    {
        var views = new List<StatementOfWorkScoreView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkScoreView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkScoreView(StatementOfWorkScoreView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedLimit;
    }
}
