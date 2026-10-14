namespace Acme.Workforce.Talent.Nomination.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination entry.</summary>
public sealed record NominationEntryView(string Code, string NameText, string Approved, string Reviewer, string FloorValueText, string Department, string HoursText, string StatusLabel)
{
    public static NominationEntryView ToNominationEntryView(NominationEntry item)
    {
        return new NominationEntryView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Approved}",
            item.Reviewer,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationEntryView> ToNominationEntryViews(List<NominationEntry> items)
    {
        var views = new List<NominationEntryView>();
        foreach (var item in items)
        {
            views.Add(ToNominationEntryView(item));
        }
        return views;
    }

    public static double SumNominationEntryView(NominationEntryView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedHours;
    }
}
