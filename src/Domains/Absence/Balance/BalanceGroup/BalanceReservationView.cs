namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using System.Globalization;

/// <summary>How screens and exports show a balance reservation.</summary>
public sealed record BalanceReservationView(string CodeText, string Title, string LabelText, string Source, string Ratio, string StatusLabel)
{
    public static BalanceReservationView ToBalanceReservationView(BalanceReservation item)
    {
        return new BalanceReservationView(
            item.Code,
            item.Title,
            item.Label,
            item.Source,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceReservationView> ToBalanceReservationViews(List<BalanceReservation> items)
    {
        var views = new List<BalanceReservationView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceReservationView(item));
        }
        return views;
    }

    public static double SumBalanceReservationView(BalanceReservationView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
