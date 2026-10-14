namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using System.Globalization;

/// <summary>How screens and exports show a primary garnishment allocation.</summary>
public sealed record PrimaryGarnishmentAllocationView(string Code, string Label, string ChannelText, string Reviewer, string Ratio, string ContactEmail, string Skill, string DurationMinutes, string Description, string Maximum, string Source, string StatusLabel)
{
    public static PrimaryGarnishmentAllocationView ToPrimaryGarnishmentAllocationView(PrimaryGarnishmentAllocation item)
    {
        return new PrimaryGarnishmentAllocationView(
            item.Code,
            item.Label,
            item.Channel,
            item.Reviewer,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Skill,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryGarnishmentAllocationView> ToPrimaryGarnishmentAllocationViews(List<PrimaryGarnishmentAllocation> items)
    {
        var views = new List<PrimaryGarnishmentAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryGarnishmentAllocationView(item));
        }
        return views;
    }

    public static double SumPrimaryGarnishmentAllocationView(PrimaryGarnishmentAllocationView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedDurationMinutes + parsedMaximum;
    }
}
