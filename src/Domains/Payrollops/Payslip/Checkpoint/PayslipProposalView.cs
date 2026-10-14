namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using System.Globalization;

/// <summary>How screens and exports show a payslip proposal.</summary>
public sealed record PayslipProposalView(string Code, string TitleText, string Skill, string BudgetText, string Minimum, string OwnerId, string Discount, string StatusLabel)
{
    public static PayslipProposalView ToPayslipProposalView(PayslipProposal item)
    {
        return new PayslipProposalView(
            item.Code,
            item.Title,
            item.Skill,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipProposalView> ToPayslipProposalViews(List<PayslipProposal> items)
    {
        var views = new List<PayslipProposalView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipProposalView(item));
        }
        return views;
    }

    public static double SumPayslipProposalView(PayslipProposalView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMinimum + parsedDiscount;
    }
}
