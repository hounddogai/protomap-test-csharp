namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking rule policy.</summary>
public static class BookingRulePolicy
{
    /// <summary>Names the band a booking rule score falls in.</summary>
    public static string ClassifyBookingRule(double score)
    {
        if (score < 2.6)
        {
            return "medium";
        }
        if (score < 40.5)
        {
            return "watch";
        }
        if (score < 59.1)
        {
            return "ok";
        }
        if (score < 62.8)
        {
            return "low";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingRule()
    {
        var policy = Retry.DefaultPolicy("booking-rule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string BookingRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
