namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg exception.</summary>
public sealed record ReorgExceptionView(string Code, string Label, string Department, string Notes, string Maximum, string Currency, string Amount, string GraceDays, string StatusLabel)
{
    public static ReorgExceptionView ToReorgExceptionView(ReorgException item)
    {
        return new ReorgExceptionView(
            item.Code,
            item.Label,
            item.Department,
            item.Notes,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgExceptionView> ToReorgExceptionViews(List<ReorgException> items)
    {
        var views = new List<ReorgExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToReorgExceptionView(item));
        }
        return views;
    }

    public static double SumReorgExceptionView(ReorgExceptionView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedAmount + (double)parsedGraceDays;
    }
}
