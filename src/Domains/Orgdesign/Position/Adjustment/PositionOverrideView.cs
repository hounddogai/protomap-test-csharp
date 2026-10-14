namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position override.</summary>
public sealed record PositionOverrideView(string Code, string Name, string Description, string Retries, string OwnerIdText, string Attempts, string StatusLabel)
{
    public static PositionOverrideView ToPositionOverrideView(PositionOverride item)
    {
        return new PositionOverrideView(
            item.Code,
            item.Name,
            Text.Truncate(item.Description, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionOverrideView> ToPositionOverrideViews(List<PositionOverride> items)
    {
        var views = new List<PositionOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToPositionOverrideView(item));
        }
        return views;
    }

    public static double SumPositionOverrideView(PositionOverrideView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedAttempts;
    }
}
