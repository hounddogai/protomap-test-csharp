namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive license seat agreement.</summary>
public sealed record AdaptiveLicenseSeatAgreementView(string CodeText, string Label, string Threshold, string Rate, string JobCode, string Pinned, string Source, string WeightText, string Sequence, string StatusLabel)
{
    public static AdaptiveLicenseSeatAgreementView ToAdaptiveLicenseSeatAgreementView(AdaptiveLicenseSeatAgreement item)
    {
        return new AdaptiveLicenseSeatAgreementView(
            item.Code,
            item.Label,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Pinned}",
            Text.Truncate(item.Source, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveLicenseSeatAgreementView> ToAdaptiveLicenseSeatAgreementViews(List<AdaptiveLicenseSeatAgreement> items)
    {
        var views = new List<AdaptiveLicenseSeatAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveLicenseSeatAgreementView(item));
        }
        return views;
    }

    public static double SumAdaptiveLicenseSeatAgreementView(AdaptiveLicenseSeatAgreementView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedRate + parsedWeight;
    }
}
