namespace Acme.Workforce.Engagement.Recognition.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition group.</summary>
public sealed record RecognitionGroupView(string Code, string Title, string Pinned, string CostText, string VisibleText, string StatusLabel)
{
    public static RecognitionGroupView ToRecognitionGroupView(RecognitionGroup item)
    {
        return new RecognitionGroupView(
            item.Code,
            Text.Truncate(item.Title, 40),
            $"{item.Pinned}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionGroupView> ToRecognitionGroupViews(List<RecognitionGroup> items)
    {
        var views = new List<RecognitionGroupView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionGroupView(item));
        }
        return views;
    }

    public static double SumRecognitionGroupView(RecognitionGroupView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
