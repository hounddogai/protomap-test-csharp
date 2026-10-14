namespace Acme.Workforce.Compliance.Certification.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled certification adjustment.</summary>
public sealed record ScheduledCertificationAdjustmentView(string Code, string NameText, string SalaryBandText, string ContactEmail, string ChannelText, string Weight, string WindowDaysText, string ManagerId, string ExpiresAtText, string StatusLabel)
{
    public static ScheduledCertificationAdjustmentView ToScheduledCertificationAdjustmentView(ScheduledCertificationAdjustment item)
    {
        return new ScheduledCertificationAdjustmentView(
            item.Code,
            item.Name,
            item.SalaryBand,
            item.ContactEmail,
            Text.Truncate(item.Channel, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledCertificationAdjustmentView> ToScheduledCertificationAdjustmentViews(List<ScheduledCertificationAdjustment> items)
    {
        var views = new List<ScheduledCertificationAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledCertificationAdjustmentView(item));
        }
        return views;
    }

    public static double SumScheduledCertificationAdjustmentView(ScheduledCertificationAdjustmentView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedWindowDays + (double)parsedExpiresAt;
    }
}
