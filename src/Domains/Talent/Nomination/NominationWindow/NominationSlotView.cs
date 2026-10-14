namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination slot.</summary>
public sealed record NominationSlotView(string Code, string Title, string Hours, string Team, string Minimum, string FteText, string LeaveDays, string Label, string StatusLabel)
{
    public static NominationSlotView ToNominationSlotView(NominationSlot item)
    {
        return new NominationSlotView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationSlotView> ToNominationSlotViews(List<NominationSlot> items)
    {
        var views = new List<NominationSlotView>();
        foreach (var item in items)
        {
            views.Add(ToNominationSlotView(item));
        }
        return views;
    }

    public static double SumNominationSlotView(NominationSlotView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedHours + parsedMinimum + parsedFte;
    }
}
