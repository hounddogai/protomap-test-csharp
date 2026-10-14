namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band reservation policy.</summary>
public static class PayBandReservationPolicy
{
    public static int PayBandReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string PayBandReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
