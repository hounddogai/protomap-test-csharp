namespace Acme.Workforce.Engagement.Feedback.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default feedback target.</summary>
public sealed record DefaultFeedbackTargetView(string Code, string TitleText, string ExternalRef, string MarginText, string Maximum, string StatusLabel)
{
    public static DefaultFeedbackTargetView ToDefaultFeedbackTargetView(DefaultFeedbackTarget item)
    {
        return new DefaultFeedbackTargetView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 40),
            item.ExternalRef,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultFeedbackTargetView> ToDefaultFeedbackTargetViews(List<DefaultFeedbackTarget> items)
    {
        var views = new List<DefaultFeedbackTargetView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultFeedbackTargetView(item));
        }
        return views;
    }

    public static double SumDefaultFeedbackTargetView(DefaultFeedbackTargetView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMargin + parsedMaximum;
    }
}
