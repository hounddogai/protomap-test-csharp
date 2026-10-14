namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using System.Globalization;

/// <summary>How screens and exports show a lenient balance checkpoint.</summary>
public sealed record LenientBalanceCheckpointView(string Code, string Title, string Hours, string CurrencyText, string CeilingText, string Discount, string EmployeeIdText, string Minimum, string Country, string ContactEmail, string SalaryBand, string StatusLabel)
{
    public static LenientBalanceCheckpointView ToLenientBalanceCheckpointView(LenientBalanceCheckpoint item)
    {
        return new LenientBalanceCheckpointView(
            item.Code,
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.ContactEmail,
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBalanceCheckpointView> ToLenientBalanceCheckpointViews(List<LenientBalanceCheckpoint> items)
    {
        var views = new List<LenientBalanceCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBalanceCheckpointView(item));
        }
        return views;
    }

    public static double SumLenientBalanceCheckpointView(LenientBalanceCheckpointView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedHours + parsedCeiling + parsedDiscount;
    }
}
