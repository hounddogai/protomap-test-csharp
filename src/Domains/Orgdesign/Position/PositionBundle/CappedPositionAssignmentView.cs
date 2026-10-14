namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped position assignment.</summary>
public sealed record CappedPositionAssignmentView(string Code, string Name, string Locale, string VisibleText, string Team, string Region, string Ratio, string Version, string ReviewerText, string Amount, string Rate, string StatusLabel)
{
    public static CappedPositionAssignmentView ToCappedPositionAssignmentView(CappedPositionAssignment item)
    {
        return new CappedPositionAssignmentView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Locale,
            $"{item.Visible}",
            item.Team,
            item.Region,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPositionAssignmentView> ToCappedPositionAssignmentViews(List<CappedPositionAssignment> items)
    {
        var views = new List<CappedPositionAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPositionAssignmentView(item));
        }
        return views;
    }

    public static double SumCappedPositionAssignmentView(CappedPositionAssignmentView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedVersion + parsedAmount;
    }
}
