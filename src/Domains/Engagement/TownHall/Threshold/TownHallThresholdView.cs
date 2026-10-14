namespace Acme.Workforce.Engagement.TownHall.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall threshold.</summary>
public sealed record TownHallThresholdView(string Code, string TitleText, string Version, string Budget, string Maximum, string AmountText, string Visible, string UpdatedAtText, string StatusLabel)
{
    public static TownHallThresholdView ToTownHallThresholdView(TownHallThreshold item)
    {
        return new TownHallThresholdView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallThresholdView> ToTownHallThresholdViews(List<TownHallThreshold> items)
    {
        var views = new List<TownHallThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallThresholdView(item));
        }
        return views;
    }

    public static double SumTownHallThresholdView(TownHallThresholdView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedBudget + parsedMaximum;
    }
}
