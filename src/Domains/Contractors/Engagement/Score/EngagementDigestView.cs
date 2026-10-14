namespace Acme.Workforce.Contractors.Engagement.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement digest.</summary>
public sealed record EngagementDigestView(string CodeText, string NameText, string Active, string LocationText, string CostCenter, string DepartmentText, string OwnerIdText, string Retries, string StatusLabel)
{
    public static EngagementDigestView ToEngagementDigestView(EngagementDigest item)
    {
        return new EngagementDigestView(
            Text.Truncate(item.Code, 64),
            item.Name,
            $"{item.Active}",
            Text.Truncate(item.Location, 40),
            Text.Truncate(item.CostCenter, 120),
            item.Department,
            item.OwnerId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementDigestView> ToEngagementDigestViews(List<EngagementDigest> items)
    {
        var views = new List<EngagementDigestView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementDigestView(item));
        }
        return views;
    }

    public static double SumEngagementDigestView(EngagementDigestView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
