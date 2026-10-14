namespace Acme.Workforce.Absence.Balance.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a balance variance.</summary>
public sealed record BalanceVarianceView(string Code, string Name, string Maximum, string AmountText, string Summary, string StatusLabel)
{
    public static BalanceVarianceView ToBalanceVarianceView(BalanceVariance item)
    {
        return new BalanceVarianceView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceVarianceView> ToBalanceVarianceViews(List<BalanceVariance> items)
    {
        var views = new List<BalanceVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceVarianceView(item));
        }
        return views;
    }

    public static double SumBalanceVarianceView(BalanceVarianceView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedAmount;
    }
}
