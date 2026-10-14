namespace Acme.Workforce.Assets.Refresh.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling refresh window.</summary>
public sealed record RollingRefreshWindowView(string CodeText, string Label, string PhoneText, string RegionText, string Weight, string Sequence, string WindowDaysText, string StatusLabel)
{
    public static RollingRefreshWindowView ToRollingRefreshWindowView(RollingRefreshWindow item)
    {
        return new RollingRefreshWindowView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Phone, 80),
            Text.Truncate(item.Region, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRefreshWindowView> ToRollingRefreshWindowViews(List<RollingRefreshWindow> items)
    {
        var views = new List<RollingRefreshWindowView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRefreshWindowView(item));
        }
        return views;
    }

    public static double SumRollingRefreshWindowView(RollingRefreshWindowView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedSequence + (double)parsedWindowDays;
    }
}
