namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant agreement.</summary>
public sealed record EquityGrantAgreementView(string Code, string Name, string Segment, string Score, string Location, string LeaveDays, string Minimum, string CapacityText, string Team, string StatusLabel)
{
    public static EquityGrantAgreementView ToEquityGrantAgreementView(EquityGrantAgreement item)
    {
        return new EquityGrantAgreementView(
            item.Code,
            item.Name,
            Text.Truncate(item.Segment, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantAgreementView> ToEquityGrantAgreementViews(List<EquityGrantAgreement> items)
    {
        var views = new List<EquityGrantAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantAgreementView(item));
        }
        return views;
    }

    public static double SumEquityGrantAgreementView(EquityGrantAgreementView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedLeaveDays + parsedMinimum;
    }
}
