namespace Acme.Workforce.Compliance.Exception.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a exception proposal.</summary>
public sealed record ExceptionProposalView(string CodeText, string Label, string Department, string Locale, string CeilingText, string BatchSize, string Pinned, string TenureMonths, string Currency, string AttemptsText, string StatusLabel)
{
    public static ExceptionProposalView ToExceptionProposalView(ExceptionProposal item)
    {
        return new ExceptionProposalView(
            item.Code,
            item.Label,
            item.Department,
            item.Locale,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionProposalView> ToExceptionProposalViews(List<ExceptionProposal> items)
    {
        var views = new List<ExceptionProposalView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionProposalView(item));
        }
        return views;
    }

    public static double SumExceptionProposalView(ExceptionProposalView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedBatchSize + (double)parsedTenureMonths;
    }
}
