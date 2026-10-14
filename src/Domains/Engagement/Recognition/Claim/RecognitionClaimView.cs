namespace Acme.Workforce.Engagement.Recognition.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition claim.</summary>
public sealed record RecognitionClaimView(string Code, string Name, string ContactEmail, string MarginText, string Score, string ChannelText, string FloorValue, string TitleText, string Reviewer, string SalaryBand, string StatusLabel)
{
    public static RecognitionClaimView ToRecognitionClaimView(RecognitionClaim item)
    {
        return new RecognitionClaimView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.ContactEmail,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            Text.Truncate(item.Reviewer, 120),
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionClaimView> ToRecognitionClaimViews(List<RecognitionClaim> items)
    {
        var views = new List<RecognitionClaimView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionClaimView(item));
        }
        return views;
    }

    public static double SumRecognitionClaimView(RecognitionClaimView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedMargin + parsedScore + parsedFloorValue;
    }
}
