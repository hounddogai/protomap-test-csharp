namespace Acme.Workforce.Assets.Refresh.RefreshReview;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy refresh profile.</summary>
public sealed record LegacyRefreshProfileView(string CodeText, string LabelText, string CostCenter, string WindowDays, string EmployeeId, string PhoneText, string DescriptionText, string LimitText, string TenureMonths, string Attempts, string Team, string StatusLabel)
{
    public static LegacyRefreshProfileView ToLegacyRefreshProfileView(LegacyRefreshProfile item)
    {
        return new LegacyRefreshProfileView(
            item.Code,
            item.Label,
            item.CostCenter,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Phone,
            Text.Truncate(item.Description, 120),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyRefreshProfileView> ToLegacyRefreshProfileViews(List<LegacyRefreshProfile> items)
    {
        var views = new List<LegacyRefreshProfileView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyRefreshProfileView(item));
        }
        return views;
    }

    public static double SumLegacyRefreshProfileView(LegacyRefreshProfileView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedLimit + (double)parsedTenureMonths;
    }
}
