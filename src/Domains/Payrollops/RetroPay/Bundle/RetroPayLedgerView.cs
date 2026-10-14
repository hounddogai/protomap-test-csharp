namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using System.Globalization;

/// <summary>How screens and exports show a retro pay ledger.</summary>
public sealed record RetroPayLedgerView(string CodeText, string Title, string Notes, string Minimum, string StartsAt, string StatusLabel)
{
    public static RetroPayLedgerView ToRetroPayLedgerView(RetroPayLedger item)
    {
        return new RetroPayLedgerView(
            item.Code,
            item.Title,
            item.Notes,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayLedgerView> ToRetroPayLedgerViews(List<RetroPayLedger> items)
    {
        var views = new List<RetroPayLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayLedgerView(item));
        }
        return views;
    }

    public static double SumRetroPayLedgerView(RetroPayLedgerView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedStartsAt;
    }
}
