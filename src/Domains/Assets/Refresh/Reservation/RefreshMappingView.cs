namespace Acme.Workforce.Assets.Refresh.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh mapping.</summary>
public sealed record RefreshMappingView(string Code, string LabelText, string BatchSizeText, string Priority, string Title, string StatusLabel)
{
    public static RefreshMappingView ToRefreshMappingView(RefreshMapping item)
    {
        return new RefreshMappingView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshMappingView> ToRefreshMappingViews(List<RefreshMapping> items)
    {
        var views = new List<RefreshMappingView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshMappingView(item));
        }
        return views;
    }

    public static double SumRefreshMappingView(RefreshMappingView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedPriority;
    }
}
