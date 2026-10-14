namespace Acme.Workforce.Assets.Assignment.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict assignment forecast.</summary>
public sealed record StrictAssignmentForecastView(string CodeText, string Name, string Badge, string CostCenterText, string Location, string ContactEmail, string BatchSize, string UpdatedAt, string Version, string StatusLabel)
{
    public static StrictAssignmentForecastView ToStrictAssignmentForecastView(StrictAssignmentForecast item)
    {
        return new StrictAssignmentForecastView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Badge,
            Text.Truncate(item.CostCenter, 40),
            Text.Truncate(item.Location, 120),
            item.ContactEmail,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAssignmentForecastView> ToStrictAssignmentForecastViews(List<StrictAssignmentForecast> items)
    {
        var views = new List<StrictAssignmentForecastView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAssignmentForecastView(item));
        }
        return views;
    }

    public static double SumStrictAssignmentForecastView(StrictAssignmentForecastView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedUpdatedAt + (double)parsedVersion;
    }
}
