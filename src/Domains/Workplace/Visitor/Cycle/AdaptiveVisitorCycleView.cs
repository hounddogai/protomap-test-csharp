namespace Acme.Workforce.Workplace.Visitor.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive visitor cycle.</summary>
public sealed record AdaptiveVisitorCycleView(string CodeText, string Title, string EmployeeId, string ContactEmail, string Retries, string ScoreText, string LeaveDays, string ChannelText, string Amount, string StatusLabel)
{
    public static AdaptiveVisitorCycleView ToAdaptiveVisitorCycleView(AdaptiveVisitorCycle item)
    {
        return new AdaptiveVisitorCycleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 64),
            item.EmployeeId,
            item.ContactEmail,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveVisitorCycleView> ToAdaptiveVisitorCycleViews(List<AdaptiveVisitorCycle> items)
    {
        var views = new List<AdaptiveVisitorCycleView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveVisitorCycleView(item));
        }
        return views;
    }

    public static double SumAdaptiveVisitorCycleView(AdaptiveVisitorCycleView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedScore + (double)parsedLeaveDays;
    }
}
