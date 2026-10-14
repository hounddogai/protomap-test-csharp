namespace Acme.Workforce.Engagement.TownHall.Notice;

using System.Globalization;

/// <summary>How screens and exports show a town hall batch.</summary>
public sealed record TownHallBatchView(string Code, string Title, string MaximumText, string EmployeeId, string UpdatedAt, string StatusLabel)
{
    public static TownHallBatchView ToTownHallBatchView(TownHallBatch item)
    {
        return new TownHallBatchView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallBatchView> ToTownHallBatchViews(List<TownHallBatch> items)
    {
        var views = new List<TownHallBatchView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallBatchView(item));
        }
        return views;
    }

    public static double SumTownHallBatchView(TownHallBatchView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedUpdatedAt;
    }
}
