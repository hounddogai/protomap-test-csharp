namespace Acme.Workforce.Talent.Nomination.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination target.</summary>
public sealed record NominationTargetView(string Code, string Name, string TitleText, string Checksum, string WeightText, string Threshold, string MinimumText, string StatusLabel)
{
    public static NominationTargetView ToNominationTargetView(NominationTarget item)
    {
        return new NominationTargetView(
            item.Code,
            item.Name,
            Text.Truncate(item.Title, 80),
            item.Checksum,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationTargetView> ToNominationTargetViews(List<NominationTarget> items)
    {
        var views = new List<NominationTargetView>();
        foreach (var item in items)
        {
            views.Add(ToNominationTargetView(item));
        }
        return views;
    }

    public static double SumNominationTargetView(NominationTargetView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedThreshold + parsedMinimum;
    }
}
