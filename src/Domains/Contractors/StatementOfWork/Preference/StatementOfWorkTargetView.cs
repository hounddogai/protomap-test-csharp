namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work target.</summary>
public sealed record StatementOfWorkTargetView(string Code, string Title, string Category, string StartsAt, string Name, string Fte, string StatusLabel)
{
    public static StatementOfWorkTargetView ToStatementOfWorkTargetView(StatementOfWorkTarget item)
    {
        return new StatementOfWorkTargetView(
            item.Code,
            item.Title,
            Text.Truncate(item.Category, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkTargetView> ToStatementOfWorkTargetViews(List<StatementOfWorkTarget> items)
    {
        var views = new List<StatementOfWorkTargetView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkTargetView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkTargetView(StatementOfWorkTargetView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedFte;
    }
}
