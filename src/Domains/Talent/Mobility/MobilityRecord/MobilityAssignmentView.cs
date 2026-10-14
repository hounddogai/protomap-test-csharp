namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using System.Globalization;

/// <summary>How screens and exports show a mobility assignment.</summary>
public sealed record MobilityAssignmentView(string Code, string Label, string DiscountText, string ExpiresAt, string RetriesText, string Rate, string CostText, string StatusLabel)
{
    public static MobilityAssignmentView ToMobilityAssignmentView(MobilityAssignment item)
    {
        return new MobilityAssignmentView(
            item.Code,
            item.Label,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityAssignmentView> ToMobilityAssignmentViews(List<MobilityAssignment> items)
    {
        var views = new List<MobilityAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityAssignmentView(item));
        }
        return views;
    }

    public static double SumMobilityAssignmentView(MobilityAssignmentView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedExpiresAt + (double)parsedRetries;
    }
}
