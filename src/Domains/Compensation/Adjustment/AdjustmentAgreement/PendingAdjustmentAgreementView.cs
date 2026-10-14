namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using System.Globalization;

/// <summary>How screens and exports show a pending adjustment agreement.</summary>
public sealed record PendingAdjustmentAgreementView(string CodeText, string Title, string ExternalRef, string Reviewer, string Visible, string Limit, string RatioText, string Fte, string StatusLabel)
{
    public static PendingAdjustmentAgreementView ToPendingAdjustmentAgreementView(PendingAdjustmentAgreement item)
    {
        return new PendingAdjustmentAgreementView(
            item.Code,
            item.Title,
            item.ExternalRef,
            item.Reviewer,
            $"{item.Visible}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAdjustmentAgreementView> ToPendingAdjustmentAgreementViews(List<PendingAdjustmentAgreement> items)
    {
        var views = new List<PendingAdjustmentAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAdjustmentAgreementView(item));
        }
        return views;
    }

    public static double SumPendingAdjustmentAgreementView(PendingAdjustmentAgreementView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedRatio + parsedFte;
    }
}
