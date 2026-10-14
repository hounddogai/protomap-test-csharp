namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using System.Globalization;

/// <summary>How screens and exports show a tiered exception group.</summary>
public sealed record TieredExceptionGroupView(string Code, string Label, string Channel, string Skill, string DurationMinutes, string RevisionText, string ManagerId, string StatusLabel)
{
    public static TieredExceptionGroupView ToTieredExceptionGroupView(TieredExceptionGroup item)
    {
        return new TieredExceptionGroupView(
            item.Code,
            item.Label,
            item.Channel,
            item.Skill,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredExceptionGroupView> ToTieredExceptionGroupViews(List<TieredExceptionGroup> items)
    {
        var views = new List<TieredExceptionGroupView>();
        foreach (var item in items)
        {
            views.Add(ToTieredExceptionGroupView(item));
        }
        return views;
    }

    public static double SumTieredExceptionGroupView(TieredExceptionGroupView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedRevision;
    }
}
