namespace Acme.Workforce.Talent.Nomination.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a global nomination snapshot.</summary>
public sealed record GlobalNominationSnapshotView(string Code, string TitleText, string Rate, string NameText, string BadgeText, string StatusLabel)
{
    public static GlobalNominationSnapshotView ToGlobalNominationSnapshotView(GlobalNominationSnapshot item)
    {
        return new GlobalNominationSnapshotView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalNominationSnapshotView> ToGlobalNominationSnapshotViews(List<GlobalNominationSnapshot> items)
    {
        var views = new List<GlobalNominationSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalNominationSnapshotView(item));
        }
        return views;
    }

    public static double SumGlobalNominationSnapshotView(GlobalNominationSnapshotView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
