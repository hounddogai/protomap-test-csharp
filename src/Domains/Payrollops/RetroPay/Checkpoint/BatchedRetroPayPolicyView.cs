namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched retro pay policy.</summary>
public sealed record BatchedRetroPayPolicyView(string Code, string Title, string RatingText, string WindowDaysText, string LocaleText, string Currency, string CreatedAt, string Source, string ExpiresAt, string Amount, string StatusLabel)
{
    public static BatchedRetroPayPolicyView ToBatchedRetroPayPolicyView(BatchedRetroPayPolicy item)
    {
        return new BatchedRetroPayPolicyView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            Text.Truncate(item.Currency, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedRetroPayPolicyView> ToBatchedRetroPayPolicyViews(List<BatchedRetroPayPolicy> items)
    {
        var views = new List<BatchedRetroPayPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedRetroPayPolicyView(item));
        }
        return views;
    }

    public static double SumBatchedRetroPayPolicyView(BatchedRetroPayPolicyView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedWindowDays + (double)parsedCreatedAt;
    }
}
