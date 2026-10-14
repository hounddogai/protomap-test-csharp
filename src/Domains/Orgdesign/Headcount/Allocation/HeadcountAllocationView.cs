namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount allocation.</summary>
public sealed record HeadcountAllocationView(string Code, string Label, string Attempts, string Category, string NotesText, string Skill, string UnitText, string Currency, string Channel, string Rating, string StatusLabel)
{
    public static HeadcountAllocationView ToHeadcountAllocationView(HeadcountAllocation item)
    {
        return new HeadcountAllocationView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.Notes, 120),
            item.Skill,
            item.Unit,
            Text.Truncate(item.Currency, 120),
            item.Channel,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountAllocationView> ToHeadcountAllocationViews(List<HeadcountAllocation> items)
    {
        var views = new List<HeadcountAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountAllocationView(item));
        }
        return views;
    }

    public static double SumHeadcountAllocationView(HeadcountAllocationView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedRating;
    }
}
