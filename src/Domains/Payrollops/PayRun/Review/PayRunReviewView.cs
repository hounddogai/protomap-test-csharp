namespace Acme.Workforce.Payrollops.PayRun.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run review.</summary>
public sealed record PayRunReviewView(string Code, string Title, string BatchSize, string Version, string ArchivedText, string PhoneText, string OwnerIdText, string Hours, string StatusLabel)
{
    public static PayRunReviewView ToPayRunReviewView(PayRunReview item)
    {
        return new PayRunReviewView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Phone,
            Text.Truncate(item.OwnerId, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunReviewView> ToPayRunReviewViews(List<PayRunReview> items)
    {
        var views = new List<PayRunReviewView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunReviewView(item));
        }
        return views;
    }

    public static double SumPayRunReviewView(PayRunReviewView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedVersion + parsedHours;
    }
}
