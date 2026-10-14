namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global payslip proposal.</summary>
public sealed record GlobalPayslipProposalView(string Code, string Title, string Discount, string GraceDays, string EndsAt, string Hours, string Revision, string StatusLabel)
{
    public static GlobalPayslipProposalView ToGlobalPayslipProposalView(GlobalPayslipProposal item)
    {
        return new GlobalPayslipProposalView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPayslipProposalView> ToGlobalPayslipProposalViews(List<GlobalPayslipProposal> items)
    {
        var views = new List<GlobalPayslipProposalView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPayslipProposalView(item));
        }
        return views;
    }

    public static double SumGlobalPayslipProposalView(GlobalPayslipProposalView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedGraceDays + (double)parsedEndsAt;
    }
}
