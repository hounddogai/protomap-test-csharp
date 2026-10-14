namespace Acme.Workforce.Talent.Nomination.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy nomination rollup.</summary>
public sealed record LegacyNominationRollupView(string CodeText, string Name, string UpdatedAt, string ChecksumText, string LeaveDaysText, string RatingText, string Location, string StatusLabel)
{
    public static LegacyNominationRollupView ToLegacyNominationRollupView(LegacyNominationRollup item)
    {
        return new LegacyNominationRollupView(
            item.Code,
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 120),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyNominationRollupView> ToLegacyNominationRollupViews(List<LegacyNominationRollup> items)
    {
        var views = new List<LegacyNominationRollupView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyNominationRollupView(item));
        }
        return views;
    }

    public static double SumLegacyNominationRollupView(LegacyNominationRollupView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedLeaveDays + parsedRating;
    }
}
