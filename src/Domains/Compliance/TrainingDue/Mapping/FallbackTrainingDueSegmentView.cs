namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using System.Globalization;

/// <summary>How screens and exports show a fallback training due segment.</summary>
public sealed record FallbackTrainingDueSegmentView(string Code, string Label, string EndsAt, string Title, string CostCenter, string MinimumText, string LimitText, string ExpiresAt, string GraceDays, string LeaveDays, string Location, string StatusLabel)
{
    public static FallbackTrainingDueSegmentView ToFallbackTrainingDueSegmentView(FallbackTrainingDueSegment item)
    {
        return new FallbackTrainingDueSegmentView(
            item.Code,
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.CostCenter,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackTrainingDueSegmentView> ToFallbackTrainingDueSegmentViews(List<FallbackTrainingDueSegment> items)
    {
        var views = new List<FallbackTrainingDueSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackTrainingDueSegmentView(item));
        }
        return views;
    }

    public static double SumFallbackTrainingDueSegmentView(FallbackTrainingDueSegmentView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMinimum + (double)parsedLimit;
    }
}
