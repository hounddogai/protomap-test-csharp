namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual equity grant group.</summary>
public sealed record ManualEquityGrantGroupView(string CodeText, string Title, string Active, string Discount, string Grade, string LimitText, string HoursText, string GraceDaysText, string Archived, string StatusLabel)
{
    public static ManualEquityGrantGroupView ToManualEquityGrantGroupView(ManualEquityGrantGroup item)
    {
        return new ManualEquityGrantGroupView(
            item.Code,
            item.Title,
            $"{item.Active}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualEquityGrantGroupView> ToManualEquityGrantGroupViews(List<ManualEquityGrantGroup> items)
    {
        var views = new List<ManualEquityGrantGroupView>();
        foreach (var item in items)
        {
            views.Add(ToManualEquityGrantGroupView(item));
        }
        return views;
    }

    public static double SumManualEquityGrantGroupView(ManualEquityGrantGroupView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedLimit + parsedHours;
    }
}
