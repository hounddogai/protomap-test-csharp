namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional statement of work digest.</summary>
public sealed record RegionalStatementOfWorkDigestView(string Code, string NameText, string Approved, string Sequence, string Channel, string GraceDays, string Title, string CurrencyText, string StatusLabel)
{
    public static RegionalStatementOfWorkDigestView ToRegionalStatementOfWorkDigestView(RegionalStatementOfWorkDigest item)
    {
        return new RegionalStatementOfWorkDigestView(
            item.Code,
            Text.Truncate(item.Name, 40),
            $"{item.Approved}",
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalStatementOfWorkDigestView> ToRegionalStatementOfWorkDigestViews(List<RegionalStatementOfWorkDigest> items)
    {
        var views = new List<RegionalStatementOfWorkDigestView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalStatementOfWorkDigestView(item));
        }
        return views;
    }

    public static double SumRegionalStatementOfWorkDigestView(RegionalStatementOfWorkDigestView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedGraceDays;
    }
}
