namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched attestation window.</summary>
public sealed record BatchedAttestationWindowView(string CodeText, string Label, string Checksum, string Description, string Skill, string Discount, string Budget, string ReviewerText, string Source, string StatusLabel)
{
    public static BatchedAttestationWindowView ToBatchedAttestationWindowView(BatchedAttestationWindow item)
    {
        return new BatchedAttestationWindowView(
            item.Code,
            item.Label,
            item.Checksum,
            Text.Truncate(item.Description, 40),
            item.Skill,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedAttestationWindowView> ToBatchedAttestationWindowViews(List<BatchedAttestationWindow> items)
    {
        var views = new List<BatchedAttestationWindowView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedAttestationWindowView(item));
        }
        return views;
    }

    public static double SumBatchedAttestationWindowView(BatchedAttestationWindowView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedBudget;
    }
}
