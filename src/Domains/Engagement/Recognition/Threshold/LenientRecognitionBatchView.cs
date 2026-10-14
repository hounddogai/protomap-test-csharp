namespace Acme.Workforce.Engagement.Recognition.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient recognition batch.</summary>
public sealed record LenientRecognitionBatchView(string CodeText, string Label, string ContactEmailText, string Pinned, string Unit, string Team, string Department, string Discount, string StatusLabel)
{
    public static LenientRecognitionBatchView ToLenientRecognitionBatchView(LenientRecognitionBatch item)
    {
        return new LenientRecognitionBatchView(
            item.Code,
            item.Label,
            item.ContactEmail,
            $"{item.Pinned}",
            Text.Truncate(item.Unit, 80),
            item.Team,
            item.Department,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientRecognitionBatchView> ToLenientRecognitionBatchViews(List<LenientRecognitionBatch> items)
    {
        var views = new List<LenientRecognitionBatchView>();
        foreach (var item in items)
        {
            views.Add(ToLenientRecognitionBatchView(item));
        }
        return views;
    }

    public static double SumLenientRecognitionBatchView(LenientRecognitionBatchView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
