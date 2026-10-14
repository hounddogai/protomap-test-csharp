namespace Acme.Workforce.Orgdesign.Position.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending position review.</summary>
public sealed record PendingPositionReviewView(string Code, string Name, string Label, string ManagerId, string ExternalRef, string Maximum, string Timezone, string SalaryBand, string Unit, string ExpiresAtText, string StatusLabel)
{
    public static PendingPositionReviewView ToPendingPositionReviewView(PendingPositionReview item)
    {
        return new PendingPositionReviewView(
            item.Code,
            item.Name,
            item.Label,
            item.ManagerId,
            Text.Truncate(item.ExternalRef, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 64),
            item.SalaryBand,
            item.Unit,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPositionReviewView> ToPendingPositionReviewViews(List<PendingPositionReview> items)
    {
        var views = new List<PendingPositionReviewView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPositionReviewView(item));
        }
        return views;
    }

    public static double SumPendingPositionReviewView(PendingPositionReviewView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedExpiresAt;
    }
}
