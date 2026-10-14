namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of garnishment reservation policy.</summary>
public static class GarnishmentReservationPolicy
{
    /// <summary>Puts a garnishment reservation code in its canonical GR- form.</summary>
    public static string NormalizeGarnishmentReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string GarnishmentReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 52799.0, in order.</summary>
    public static int GarnishmentReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52799.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
