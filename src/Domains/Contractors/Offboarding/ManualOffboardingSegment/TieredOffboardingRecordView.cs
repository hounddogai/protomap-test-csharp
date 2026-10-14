namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using System.Globalization;

/// <summary>How screens and exports show a tiered offboarding record.</summary>
public sealed record TieredOffboardingRecordView(string CodeText, string Name, string Archived, string AmountText, string Region, string Ratio, string StatusLabel)
{
    public static TieredOffboardingRecordView ToTieredOffboardingRecordView(TieredOffboardingRecord item)
    {
        return new TieredOffboardingRecordView(
            item.Code,
            item.Name,
            $"{item.Archived}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredOffboardingRecordView> ToTieredOffboardingRecordViews(List<TieredOffboardingRecord> items)
    {
        var views = new List<TieredOffboardingRecordView>();
        foreach (var item in items)
        {
            views.Add(ToTieredOffboardingRecordView(item));
        }
        return views;
    }

    public static double SumTieredOffboardingRecordView(TieredOffboardingRecordView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedAmount + parsedRatio;
    }
}
