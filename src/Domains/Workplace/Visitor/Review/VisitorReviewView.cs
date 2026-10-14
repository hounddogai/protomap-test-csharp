namespace Acme.Workforce.Workplace.Visitor.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor review.</summary>
public sealed record VisitorReviewView(string CodeText, string Name, string Description, string Checksum, string PriorityText, string Active, string BatchSizeText, string Badge, string Department, string ExpiresAt, string StatusLabel)
{
    public static VisitorReviewView ToVisitorReviewView(VisitorReview item)
    {
        return new VisitorReviewView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 40),
            item.Description,
            item.Checksum,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 64),
            Text.Truncate(item.Department, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorReviewView> ToVisitorReviewViews(List<VisitorReview> items)
    {
        var views = new List<VisitorReviewView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorReviewView(item));
        }
        return views;
    }

    public static double SumVisitorReviewView(VisitorReviewView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedBatchSize + (double)parsedExpiresAt;
    }
}
