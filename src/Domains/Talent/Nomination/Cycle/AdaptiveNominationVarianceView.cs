namespace Acme.Workforce.Talent.Nomination.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive nomination variance.</summary>
public sealed record AdaptiveNominationVarianceView(string Code, string LabelText, string DurationMinutesText, string Source, string PriorityText, string UpdatedAt, string BatchSize, string StatusLabel)
{
    public static AdaptiveNominationVarianceView ToAdaptiveNominationVarianceView(AdaptiveNominationVariance item)
    {
        return new AdaptiveNominationVarianceView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveNominationVarianceView> ToAdaptiveNominationVarianceViews(List<AdaptiveNominationVariance> items)
    {
        var views = new List<AdaptiveNominationVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveNominationVarianceView(item));
        }
        return views;
    }

    public static double SumAdaptiveNominationVarianceView(AdaptiveNominationVarianceView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedPriority + (double)parsedUpdatedAt;
    }
}
