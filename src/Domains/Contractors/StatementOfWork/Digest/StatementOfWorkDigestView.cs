namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work digest.</summary>
public sealed record StatementOfWorkDigestView(string CodeText, string NameText, string Priority, string SalaryBand, string RegionText, string Rate, string Currency, string EndsAt, string Checksum, string StatusLabel)
{
    public static StatementOfWorkDigestView ToStatementOfWorkDigestView(StatementOfWorkDigest item)
    {
        return new StatementOfWorkDigestView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            item.Region,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkDigestView> ToStatementOfWorkDigestViews(List<StatementOfWorkDigest> items)
    {
        var views = new List<StatementOfWorkDigestView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkDigestView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkDigestView(StatementOfWorkDigestView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedRate + (double)parsedEndsAt;
    }
}
