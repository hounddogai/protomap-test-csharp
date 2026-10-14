namespace Acme.Workforce.Talent.Nomination.Ledger;

using System.Globalization;

/// <summary>How screens and exports show a nomination schedule.</summary>
public sealed record NominationScheduleView(string Code, string NameText, string ThresholdText, string Phone, string StatusLabel)
{
    public static NominationScheduleView ToNominationScheduleView(NominationSchedule item)
    {
        return new NominationScheduleView(
            item.Code,
            item.Name,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationScheduleView> ToNominationScheduleViews(List<NominationSchedule> items)
    {
        var views = new List<NominationScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToNominationScheduleView(item));
        }
        return views;
    }

    public static double SumNominationScheduleView(NominationScheduleView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
