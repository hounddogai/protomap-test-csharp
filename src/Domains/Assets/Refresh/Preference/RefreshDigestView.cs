namespace Acme.Workforce.Assets.Refresh.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh digest.</summary>
public sealed record RefreshDigestView(string Code, string Title, string Maximum, string Weight, string ReviewerText, string Badge, string StatusLabel)
{
    public static RefreshDigestView ToRefreshDigestView(RefreshDigest item)
    {
        return new RefreshDigestView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            Text.Truncate(item.Badge, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshDigestView> ToRefreshDigestViews(List<RefreshDigest> items)
    {
        var views = new List<RefreshDigestView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshDigestView(item));
        }
        return views;
    }

    public static double SumRefreshDigestView(RefreshDigestView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedWeight;
    }
}
