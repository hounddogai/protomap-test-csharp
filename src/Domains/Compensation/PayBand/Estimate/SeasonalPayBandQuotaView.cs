namespace Acme.Workforce.Compensation.PayBand.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal pay band quota.</summary>
public sealed record SeasonalPayBandQuotaView(string Code, string LabelText, string JobCode, string EmployeeId, string GraceDays, string Sequence, string LeaveDays, string Region, string StatusLabel)
{
    public static SeasonalPayBandQuotaView ToSeasonalPayBandQuotaView(SeasonalPayBandQuota item)
    {
        return new SeasonalPayBandQuotaView(
            Text.Truncate(item.Code, 40),
            item.Label,
            Text.Truncate(item.JobCode, 120),
            item.EmployeeId,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPayBandQuotaView> ToSeasonalPayBandQuotaViews(List<SeasonalPayBandQuota> items)
    {
        var views = new List<SeasonalPayBandQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPayBandQuotaView(item));
        }
        return views;
    }

    public static double SumSeasonalPayBandQuotaView(SeasonalPayBandQuotaView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedSequence + (double)parsedLeaveDays;
    }
}
