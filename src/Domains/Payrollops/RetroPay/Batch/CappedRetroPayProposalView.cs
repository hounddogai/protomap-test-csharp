namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped retro pay proposal.</summary>
public sealed record CappedRetroPayProposalView(string Code, string Title, string MaximumText, string DurationMinutes, string Budget, string SourceText, string Revision, string WeightText, string ExternalRef, string Checksum, string StatusLabel)
{
    public static CappedRetroPayProposalView ToCappedRetroPayProposalView(CappedRetroPayProposal item)
    {
        return new CappedRetroPayProposalView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            Text.Truncate(item.Checksum, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedRetroPayProposalView> ToCappedRetroPayProposalViews(List<CappedRetroPayProposal> items)
    {
        var views = new List<CappedRetroPayProposalView>();
        foreach (var item in items)
        {
            views.Add(ToCappedRetroPayProposalView(item));
        }
        return views;
    }

    public static double SumCappedRetroPayProposalView(CappedRetroPayProposalView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedDurationMinutes + parsedBudget;
    }
}
