namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form proposal.</summary>
public sealed record TaxFormProposalView(string Code, string LabelText, string Active, string ApprovedText, string Visible, string Score, string StatusLabel)
{
    public static TaxFormProposalView ToTaxFormProposalView(TaxFormProposal item)
    {
        return new TaxFormProposalView(
            Text.Truncate(item.Code, 120),
            item.Label,
            $"{item.Active}",
            $"{item.Approved}",
            $"{item.Visible}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormProposalView> ToTaxFormProposalViews(List<TaxFormProposal> items)
    {
        var views = new List<TaxFormProposalView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormProposalView(item));
        }
        return views;
    }

    public static double SumTaxFormProposalView(TaxFormProposalView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
