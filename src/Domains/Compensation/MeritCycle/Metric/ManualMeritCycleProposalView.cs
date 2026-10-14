namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using System.Globalization;

/// <summary>How screens and exports show a manual merit cycle proposal.</summary>
public sealed record ManualMeritCycleProposalView(string Code, string Label, string FloorValue, string ExpiresAtText, string HoursText, string Country, string StatusLabel)
{
    public static ManualMeritCycleProposalView ToManualMeritCycleProposalView(ManualMeritCycleProposal item)
    {
        return new ManualMeritCycleProposalView(
            item.Code,
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualMeritCycleProposalView> ToManualMeritCycleProposalViews(List<ManualMeritCycleProposal> items)
    {
        var views = new List<ManualMeritCycleProposalView>();
        foreach (var item in items)
        {
            views.Add(ToManualMeritCycleProposalView(item));
        }
        return views;
    }

    public static double SumManualMeritCycleProposalView(ManualMeritCycleProposalView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedExpiresAt + parsedHours;
    }
}
