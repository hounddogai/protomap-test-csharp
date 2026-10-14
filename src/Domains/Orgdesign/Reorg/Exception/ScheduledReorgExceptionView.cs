namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled reorg exception.</summary>
public sealed record ScheduledReorgExceptionView(string Code, string Name, string Region, string AttemptsText, string Limit, string WindowDaysText, string StatusLabel)
{
    public static ScheduledReorgExceptionView ToScheduledReorgExceptionView(ScheduledReorgException item)
    {
        return new ScheduledReorgExceptionView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Region,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledReorgExceptionView> ToScheduledReorgExceptionViews(List<ScheduledReorgException> items)
    {
        var views = new List<ScheduledReorgExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledReorgExceptionView(item));
        }
        return views;
    }

    public static double SumScheduledReorgExceptionView(ScheduledReorgExceptionView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedLimit + (double)parsedWindowDays;
    }
}
