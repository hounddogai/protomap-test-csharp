namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a seasonal offboarding slot.</summary>
public sealed record SeasonalOffboardingSlotView(string Code, string NameText, string Active, string JobCode, string ContactEmail, string Timezone, string Sequence, string BatchSize, string Maximum, string SalaryBand, string LimitText, string StatusLabel)
{
    public static SeasonalOffboardingSlotView ToSeasonalOffboardingSlotView(SeasonalOffboardingSlot item)
    {
        return new SeasonalOffboardingSlotView(
            item.Code,
            item.Name,
            $"{item.Active}",
            item.JobCode,
            item.ContactEmail,
            item.Timezone,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalOffboardingSlotView> ToSeasonalOffboardingSlotViews(List<SeasonalOffboardingSlot> items)
    {
        var views = new List<SeasonalOffboardingSlotView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalOffboardingSlotView(item));
        }
        return views;
    }

    public static double SumSeasonalOffboardingSlotView(SeasonalOffboardingSlotView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedBatchSize + parsedMaximum;
    }
}
