namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work profile.</summary>
public sealed record StatementOfWorkProfileView(string Code, string Label, string ExpiresAt, string Threshold, string Badge, string StatusLabel)
{
    public static StatementOfWorkProfileView ToStatementOfWorkProfileView(StatementOfWorkProfile item)
    {
        return new StatementOfWorkProfileView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 40),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkProfileView> ToStatementOfWorkProfileViews(List<StatementOfWorkProfile> items)
    {
        var views = new List<StatementOfWorkProfileView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkProfileView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkProfileView(StatementOfWorkProfileView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedThreshold;
    }
}
