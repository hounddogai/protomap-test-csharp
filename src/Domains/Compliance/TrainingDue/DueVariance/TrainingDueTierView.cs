namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using System.Globalization;

/// <summary>How screens and exports show a training due tier.</summary>
public sealed record TrainingDueTierView(string Code, string Label, string UpdatedAt, string Department, string Category, string Channel, string PinnedText, string StatusLabel)
{
    public static TrainingDueTierView ToTrainingDueTierView(TrainingDueTier item)
    {
        return new TrainingDueTierView(
            item.Code,
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Category,
            item.Channel,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueTierView> ToTrainingDueTierViews(List<TrainingDueTier> items)
    {
        var views = new List<TrainingDueTierView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueTierView(item));
        }
        return views;
    }

    public static double SumTrainingDueTierView(TrainingDueTierView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt;
    }
}
