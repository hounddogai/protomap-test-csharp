namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work rollup.</summary>
public sealed record StatementOfWorkRollupView(string CodeText, string LabelText, string Region, string Limit, string Skill, string ExpiresAt, string StatusLabel)
{
    public static StatementOfWorkRollupView ToStatementOfWorkRollupView(StatementOfWorkRollup item)
    {
        return new StatementOfWorkRollupView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 40),
            item.Region,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 40),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkRollupView> ToStatementOfWorkRollupViews(List<StatementOfWorkRollup> items)
    {
        var views = new List<StatementOfWorkRollupView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkRollupView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkRollupView(StatementOfWorkRollupView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedExpiresAt;
    }
}
