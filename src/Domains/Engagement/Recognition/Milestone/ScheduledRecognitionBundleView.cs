namespace Acme.Workforce.Engagement.Recognition.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled recognition bundle.</summary>
public sealed record ScheduledRecognitionBundleView(string Code, string Label, string Hours, string GradeText, string Quantity, string Ceiling, string Revision, string PinnedText, string TenureMonths, string StatusLabel)
{
    public static ScheduledRecognitionBundleView ToScheduledRecognitionBundleView(ScheduledRecognitionBundle item)
    {
        return new ScheduledRecognitionBundleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRecognitionBundleView> ToScheduledRecognitionBundleViews(List<ScheduledRecognitionBundle> items)
    {
        var views = new List<ScheduledRecognitionBundleView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRecognitionBundleView(item));
        }
        return views;
    }

    public static double SumScheduledRecognitionBundleView(ScheduledRecognitionBundleView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedQuantity + parsedCeiling;
    }
}
