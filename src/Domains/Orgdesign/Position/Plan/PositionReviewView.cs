namespace Acme.Workforce.Orgdesign.Position.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position review.</summary>
public sealed record PositionReviewView(string CodeText, string Name, string Retries, string Pinned, string Channel, string Capacity, string Discount, string WindowDays, string ContactEmail, string BatchSize, string DepartmentText, string StatusLabel)
{
    public static PositionReviewView ToPositionReviewView(PositionReview item)
    {
        return new PositionReviewView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Channel,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionReviewView> ToPositionReviewViews(List<PositionReview> items)
    {
        var views = new List<PositionReviewView>();
        foreach (var item in items)
        {
            views.Add(ToPositionReviewView(item));
        }
        return views;
    }

    public static double SumPositionReviewView(PositionReviewView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedCapacity + parsedDiscount;
    }
}
