namespace Acme.Workforce.Compensation.BonusPool.Review;

using System.Globalization;

/// <summary>How screens and exports show a default bonus pool allocation.</summary>
public sealed record DefaultBonusPoolAllocationView(string Code, string NameText, string Description, string VersionText, string Team, string Priority, string Attempts, string Maximum, string Grade, string StatusLabel)
{
    public static DefaultBonusPoolAllocationView ToDefaultBonusPoolAllocationView(DefaultBonusPoolAllocation item)
    {
        return new DefaultBonusPoolAllocationView(
            item.Code,
            item.Name,
            item.Description,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultBonusPoolAllocationView> ToDefaultBonusPoolAllocationViews(List<DefaultBonusPoolAllocation> items)
    {
        var views = new List<DefaultBonusPoolAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultBonusPoolAllocationView(item));
        }
        return views;
    }

    public static double SumDefaultBonusPoolAllocationView(DefaultBonusPoolAllocationView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedPriority + (double)parsedAttempts;
    }
}
