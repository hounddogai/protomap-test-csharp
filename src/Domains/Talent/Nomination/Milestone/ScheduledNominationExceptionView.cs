namespace Acme.Workforce.Talent.Nomination.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a scheduled nomination exception.</summary>
public sealed record ScheduledNominationExceptionView(string CodeText, string Title, string Maximum, string ManagerId, string StatusLabel)
{
    public static ScheduledNominationExceptionView ToScheduledNominationExceptionView(ScheduledNominationException item)
    {
        return new ScheduledNominationExceptionView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledNominationExceptionView> ToScheduledNominationExceptionViews(List<ScheduledNominationException> items)
    {
        var views = new List<ScheduledNominationExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledNominationExceptionView(item));
        }
        return views;
    }

    public static double SumScheduledNominationExceptionView(ScheduledNominationExceptionView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
