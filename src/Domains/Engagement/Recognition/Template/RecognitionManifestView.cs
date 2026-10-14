namespace Acme.Workforce.Engagement.Recognition.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition manifest.</summary>
public sealed record RecognitionManifestView(string Code, string Label, string PinnedText, string Ratio, string Unit, string FloorValue, string Amount, string Archived, string Title, string StatusLabel)
{
    public static RecognitionManifestView ToRecognitionManifestView(RecognitionManifest item)
    {
        return new RecognitionManifestView(
            item.Code,
            item.Label,
            $"{item.Pinned}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Title, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionManifestView> ToRecognitionManifestViews(List<RecognitionManifest> items)
    {
        var views = new List<RecognitionManifestView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionManifestView(item));
        }
        return views;
    }

    public static double SumRecognitionManifestView(RecognitionManifestView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedRatio + parsedFloorValue + parsedAmount;
    }
}
