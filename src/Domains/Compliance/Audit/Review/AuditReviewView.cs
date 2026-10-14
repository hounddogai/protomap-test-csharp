namespace Acme.Workforce.Compliance.Audit.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit review.</summary>
public sealed record AuditReviewView(string Code, string TitleText, string CostCenterText, string Rating, string VisibleText, string Region, string OwnerIdText, string Summary, string Reviewer, string DepartmentText, string Cost, string StatusLabel)
{
    public static AuditReviewView ToAuditReviewView(AuditReview item)
    {
        return new AuditReviewView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.CostCenter,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            Text.Truncate(item.Region, 40),
            item.OwnerId,
            item.Summary,
            Text.Truncate(item.Reviewer, 40),
            item.Department,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditReviewView> ToAuditReviewViews(List<AuditReview> items)
    {
        var views = new List<AuditReviewView>();
        foreach (var item in items)
        {
            views.Add(ToAuditReviewView(item));
        }
        return views;
    }

    public static double SumAuditReviewView(AuditReviewView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedRating + parsedCost;
    }
}
