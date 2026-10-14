namespace Acme.Workforce.Absence.Balance.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance cycle.</summary>
public sealed record BalanceCycleView(string CodeText, string Title, string Attempts, string Retries, string Threshold, string OwnerId, string StatusLabel)
{
    public static BalanceCycleView ToBalanceCycleView(BalanceCycle item)
    {
        return new BalanceCycleView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceCycleView> ToBalanceCycleViews(List<BalanceCycle> items)
    {
        var views = new List<BalanceCycleView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceCycleView(item));
        }
        return views;
    }

    public static double SumBalanceCycleView(BalanceCycleView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedRetries + parsedThreshold;
    }
}
