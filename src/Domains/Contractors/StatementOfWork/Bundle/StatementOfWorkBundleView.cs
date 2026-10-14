namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work bundle.</summary>
public sealed record StatementOfWorkBundleView(string Code, string Name, string UpdatedAt, string Channel, string Budget, string StatusLabel)
{
    public static StatementOfWorkBundleView ToStatementOfWorkBundleView(StatementOfWorkBundle item)
    {
        return new StatementOfWorkBundleView(
            item.Code,
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 120),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkBundleView> ToStatementOfWorkBundleViews(List<StatementOfWorkBundle> items)
    {
        var views = new List<StatementOfWorkBundleView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkBundleView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkBundleView(StatementOfWorkBundleView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedBudget;
    }
}
