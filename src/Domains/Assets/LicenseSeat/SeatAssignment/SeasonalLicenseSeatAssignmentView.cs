namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using System.Globalization;

/// <summary>How screens and exports show a seasonal license seat assignment.</summary>
public sealed record SeasonalLicenseSeatAssignmentView(string Code, string Name, string Team, string CostText, string Sequence, string StatusLabel)
{
    public static SeasonalLicenseSeatAssignmentView ToSeasonalLicenseSeatAssignmentView(SeasonalLicenseSeatAssignment item)
    {
        return new SeasonalLicenseSeatAssignmentView(
            item.Code,
            item.Name,
            item.Team,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalLicenseSeatAssignmentView> ToSeasonalLicenseSeatAssignmentViews(List<SeasonalLicenseSeatAssignment> items)
    {
        var views = new List<SeasonalLicenseSeatAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalLicenseSeatAssignmentView(item));
        }
        return views;
    }

    public static double SumSeasonalLicenseSeatAssignmentView(SeasonalLicenseSeatAssignmentView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedSequence;
    }
}
