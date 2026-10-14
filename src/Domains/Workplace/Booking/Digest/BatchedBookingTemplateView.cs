namespace Acme.Workforce.Workplace.Booking.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched booking template.</summary>
public sealed record BatchedBookingTemplateView(string CodeText, string Label, string JobCodeText, string SalaryBand, string Pinned, string RateText, string Approved, string Visible, string Archived, string UpdatedAtText, string StatusLabel)
{
    public static BatchedBookingTemplateView ToBatchedBookingTemplateView(BatchedBookingTemplate item)
    {
        return new BatchedBookingTemplateView(
            item.Code,
            item.Label,
            item.JobCode,
            Text.Truncate(item.SalaryBand, 40),
            $"{item.Pinned}",
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Visible}",
            $"{item.Archived}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedBookingTemplateView> ToBatchedBookingTemplateViews(List<BatchedBookingTemplate> items)
    {
        var views = new List<BatchedBookingTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedBookingTemplateView(item));
        }
        return views;
    }

    public static double SumBatchedBookingTemplateView(BatchedBookingTemplateView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedUpdatedAt;
    }
}
