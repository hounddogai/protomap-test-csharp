namespace Acme.Workforce.Talent.CareerPath.Rule;

using System.Globalization;

/// <summary>How screens and exports show a career path rule.</summary>
public sealed record CareerPathRuleView(string Code, string TitleText, string SequenceText, string Timezone, string ThresholdText, string Grade, string StatusLabel)
{
    public static CareerPathRuleView ToCareerPathRuleView(CareerPathRule item)
    {
        return new CareerPathRuleView(
            item.Code,
            item.Title,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathRuleView> ToCareerPathRuleViews(List<CareerPathRule> items)
    {
        var views = new List<CareerPathRuleView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathRuleView(item));
        }
        return views;
    }

    public static double SumCareerPathRuleView(CareerPathRuleView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedThreshold;
    }
}
