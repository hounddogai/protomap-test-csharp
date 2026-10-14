namespace Acme.Workforce.Absence.Balance.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback balance cycle.</summary>
public sealed record FallbackBalanceCycleView(string Code, string LabelText, string Active, string ChecksumText, string CreatedAtText, string AttemptsText, string Phone, string AmountText, string Version, string Ratio, string StatusLabel)
{
    public static FallbackBalanceCycleView ToFallbackBalanceCycleView(FallbackBalanceCycle item)
    {
        return new FallbackBalanceCycleView(
            item.Code,
            item.Label,
            $"{item.Active}",
            item.Checksum,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackBalanceCycleView> ToFallbackBalanceCycleViews(List<FallbackBalanceCycle> items)
    {
        var views = new List<FallbackBalanceCycleView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackBalanceCycleView(item));
        }
        return views;
    }

    public static double SumFallbackBalanceCycleView(FallbackBalanceCycleView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedAttempts + parsedAmount;
    }
}
