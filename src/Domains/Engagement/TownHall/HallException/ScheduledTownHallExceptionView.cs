namespace Acme.Workforce.Engagement.TownHall.HallException;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled town hall exception.</summary>
public sealed record ScheduledTownHallExceptionView(string Code, string Name, string JobCodeText, string Revision, string Rating, string Attempts, string Title, string VersionText, string StatusLabel)
{
    public static ScheduledTownHallExceptionView ToScheduledTownHallExceptionView(ScheduledTownHallException item)
    {
        return new ScheduledTownHallExceptionView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.JobCode,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledTownHallExceptionView> ToScheduledTownHallExceptionViews(List<ScheduledTownHallException> items)
    {
        var views = new List<ScheduledTownHallExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledTownHallExceptionView(item));
        }
        return views;
    }

    public static double SumScheduledTownHallExceptionView(ScheduledTownHallExceptionView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedRating + (double)parsedAttempts;
    }
}
