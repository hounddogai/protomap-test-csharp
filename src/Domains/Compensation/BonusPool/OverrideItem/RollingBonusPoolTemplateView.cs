namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a rolling bonus pool template.</summary>
public sealed record RollingBonusPoolTemplateView(string CodeText, string LabelText, string WeightText, string DurationMinutes, string Reviewer, string UnitText, string Attempts, string Ratio, string StatusLabel)
{
    public static RollingBonusPoolTemplateView ToRollingBonusPoolTemplateView(RollingBonusPoolTemplate item)
    {
        return new RollingBonusPoolTemplateView(
            item.Code,
            item.Label,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Unit,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingBonusPoolTemplateView> ToRollingBonusPoolTemplateViews(List<RollingBonusPoolTemplate> items)
    {
        var views = new List<RollingBonusPoolTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToRollingBonusPoolTemplateView(item));
        }
        return views;
    }

    public static double SumRollingBonusPoolTemplateView(RollingBonusPoolTemplateView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedDurationMinutes + (double)parsedAttempts;
    }
}
