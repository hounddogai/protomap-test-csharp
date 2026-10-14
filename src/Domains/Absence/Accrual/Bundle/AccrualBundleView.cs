namespace Acme.Workforce.Absence.Accrual.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual bundle.</summary>
public sealed record AccrualBundleView(string Code, string Name, string ExpiresAt, string Channel, string GraceDays, string ContactEmailText, string Grade, string Limit, string FloorValue, string StatusLabel)
{
    public static AccrualBundleView ToAccrualBundleView(AccrualBundle item)
    {
        return new AccrualBundleView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Grade,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualBundleView> ToAccrualBundleViews(List<AccrualBundle> items)
    {
        var views = new List<AccrualBundleView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualBundleView(item));
        }
        return views;
    }

    public static double SumAccrualBundleView(AccrualBundleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedGraceDays + (double)parsedLimit;
    }
}
