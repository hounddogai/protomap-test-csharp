namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount entry.</summary>
public sealed record HeadcountEntryView(string Code, string LabelText, string Channel, string DurationMinutes, string ContactEmailText, string ManagerId, string StatusLabel)
{
    public static HeadcountEntryView ToHeadcountEntryView(HeadcountEntry item)
    {
        return new HeadcountEntryView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Channel,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountEntryView> ToHeadcountEntryViews(List<HeadcountEntry> items)
    {
        var views = new List<HeadcountEntryView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountEntryView(item));
        }
        return views;
    }

    public static double SumHeadcountEntryView(HeadcountEntryView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
