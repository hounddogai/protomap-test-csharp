namespace Acme.Workforce.Assets.Warranty.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled warranty cycle.</summary>
public sealed record ScheduledWarrantyCycleView(string Code, string Name, string Checksum, string Margin, string Hours, string WeightText, string Budget, string StatusLabel)
{
    public static ScheduledWarrantyCycleView ToScheduledWarrantyCycleView(ScheduledWarrantyCycle item)
    {
        return new ScheduledWarrantyCycleView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Checksum,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledWarrantyCycleView> ToScheduledWarrantyCycleViews(List<ScheduledWarrantyCycle> items)
    {
        var views = new List<ScheduledWarrantyCycleView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledWarrantyCycleView(item));
        }
        return views;
    }

    public static double SumScheduledWarrantyCycleView(ScheduledWarrantyCycleView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedMargin + parsedHours + parsedWeight;
    }
}
