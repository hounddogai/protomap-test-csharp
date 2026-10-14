namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work override.</summary>
public sealed record StatementOfWorkOverrideView(string CodeText, string Label, string Currency, string Country, string Department, string Badge, string CreatedAt, string BatchSizeText, string Summary, string StatusLabel)
{
    public static StatementOfWorkOverrideView ToStatementOfWorkOverrideView(StatementOfWorkOverride item)
    {
        return new StatementOfWorkOverrideView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Currency, 64),
            item.Country,
            item.Department,
            item.Badge,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkOverrideView> ToStatementOfWorkOverrideViews(List<StatementOfWorkOverride> items)
    {
        var views = new List<StatementOfWorkOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkOverrideView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkOverrideView(StatementOfWorkOverrideView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedBatchSize;
    }
}
