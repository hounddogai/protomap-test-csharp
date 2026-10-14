namespace Acme.Workforce.Payrollops.Deduction.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction tier.</summary>
public sealed record DeductionTierView(string Code, string TitleText, string Weight, string Version, string Rate, string Phone, string RetriesText, string FloorValue, string SequenceText, string Visible, string Checksum, string StatusLabel)
{
    public static DeductionTierView ToDeductionTierView(DeductionTier item)
    {
        return new DeductionTierView(
            item.Code,
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            Text.Truncate(item.Checksum, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionTierView> ToDeductionTierViews(List<DeductionTier> items)
    {
        var views = new List<DeductionTierView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionTierView(item));
        }
        return views;
    }

    public static double SumDeductionTierView(DeductionTierView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedVersion + parsedRate;
    }
}
