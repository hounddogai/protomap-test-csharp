namespace Acme.Workforce.Orgdesign.Position.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual position summary.</summary>
public sealed record ManualPositionSummaryView(string CodeText, string TitleText, string FloorValueText, string AmountText, string Active, string ApprovedText, string ContactEmailText, string Team, string SalaryBand, string RateText, string StatusLabel)
{
    public static ManualPositionSummaryView ToManualPositionSummaryView(ManualPositionSummary item)
    {
        return new ManualPositionSummaryView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Approved}",
            item.ContactEmail,
            Text.Truncate(item.Team, 120),
            item.SalaryBand,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPositionSummaryView> ToManualPositionSummaryViews(List<ManualPositionSummary> items)
    {
        var views = new List<ManualPositionSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToManualPositionSummaryView(item));
        }
        return views;
    }

    public static double SumManualPositionSummaryView(ManualPositionSummaryView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedAmount + parsedRate;
    }
}
