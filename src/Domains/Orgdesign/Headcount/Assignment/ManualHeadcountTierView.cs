namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual headcount tier.</summary>
public sealed record ManualHeadcountTierView(string Code, string Label, string UpdatedAt, string LocationText, string EmployeeId, string GraceDays, string StatusLabel)
{
    public static ManualHeadcountTierView ToManualHeadcountTierView(ManualHeadcountTier item)
    {
        return new ManualHeadcountTierView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.EmployeeId, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualHeadcountTierView> ToManualHeadcountTierViews(List<ManualHeadcountTier> items)
    {
        var views = new List<ManualHeadcountTierView>();
        foreach (var item in items)
        {
            views.Add(ToManualHeadcountTierView(item));
        }
        return views;
    }

    public static double SumManualHeadcountTierView(ManualHeadcountTierView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedGraceDays;
    }
}
