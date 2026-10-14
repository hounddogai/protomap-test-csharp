namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work variance.</summary>
public sealed record StatementOfWorkVarianceView(string Code, string Title, string Country, string Category, string Region, string Amount, string StatusLabel)
{
    public static StatementOfWorkVarianceView ToStatementOfWorkVarianceView(StatementOfWorkVariance item)
    {
        return new StatementOfWorkVarianceView(
            item.Code,
            item.Title,
            item.Country,
            Text.Truncate(item.Category, 120),
            item.Region,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkVarianceView> ToStatementOfWorkVarianceViews(List<StatementOfWorkVariance> items)
    {
        var views = new List<StatementOfWorkVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkVarianceView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkVarianceView(StatementOfWorkVarianceView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
