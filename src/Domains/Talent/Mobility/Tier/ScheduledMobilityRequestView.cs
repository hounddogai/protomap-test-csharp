namespace Acme.Workforce.Talent.Mobility.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled mobility request.</summary>
public sealed record ScheduledMobilityRequestView(string CodeText, string NameText, string EmployeeIdText, string Approved, string ActiveText, string Archived, string AmountText, string StatusLabel)
{
    public static ScheduledMobilityRequestView ToScheduledMobilityRequestView(ScheduledMobilityRequest item)
    {
        return new ScheduledMobilityRequestView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.EmployeeId, 120),
            $"{item.Approved}",
            $"{item.Active}",
            $"{item.Archived}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledMobilityRequestView> ToScheduledMobilityRequestViews(List<ScheduledMobilityRequest> items)
    {
        var views = new List<ScheduledMobilityRequestView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledMobilityRequestView(item));
        }
        return views;
    }

    public static double SumScheduledMobilityRequestView(ScheduledMobilityRequestView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
