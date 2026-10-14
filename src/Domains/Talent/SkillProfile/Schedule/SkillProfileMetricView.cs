namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using System.Globalization;

/// <summary>How screens and exports show a skill profile metric.</summary>
public sealed record SkillProfileMetricView(string Code, string Title, string Hours, string VisibleText, string FloorValueText, string ApprovedText, string ThresholdText, string LocaleText, string Discount, string StatusLabel)
{
    public static SkillProfileMetricView ToSkillProfileMetricView(SkillProfileMetric item)
    {
        return new SkillProfileMetricView(
            item.Code,
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileMetricView> ToSkillProfileMetricViews(List<SkillProfileMetric> items)
    {
        var views = new List<SkillProfileMetricView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileMetricView(item));
        }
        return views;
    }

    public static double SumSkillProfileMetricView(SkillProfileMetricView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedHours + parsedFloorValue + parsedThreshold;
    }
}
