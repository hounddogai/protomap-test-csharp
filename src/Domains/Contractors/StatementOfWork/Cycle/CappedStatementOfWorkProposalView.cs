namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped statement of work proposal.</summary>
public sealed record CappedStatementOfWorkProposalView(string Code, string Name, string LimitText, string Ceiling, string EmployeeId, string DiscountText, string Department, string BatchSize, string StatusLabel)
{
    public static CappedStatementOfWorkProposalView ToCappedStatementOfWorkProposalView(CappedStatementOfWorkProposal item)
    {
        return new CappedStatementOfWorkProposalView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedStatementOfWorkProposalView> ToCappedStatementOfWorkProposalViews(List<CappedStatementOfWorkProposal> items)
    {
        var views = new List<CappedStatementOfWorkProposalView>();
        foreach (var item in items)
        {
            views.Add(ToCappedStatementOfWorkProposalView(item));
        }
        return views;
    }

    public static double SumCappedStatementOfWorkProposalView(CappedStatementOfWorkProposalView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedCeiling + parsedDiscount;
    }
}
