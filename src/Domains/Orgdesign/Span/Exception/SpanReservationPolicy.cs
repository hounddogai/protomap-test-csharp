namespace Acme.Workforce.Orgdesign.Span.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of span reservation policy.</summary>
public static class SpanReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSpanReservation()
    {
        var policy = Retry.DefaultPolicy("span-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 31081.0, in order.</summary>
    public static int SpanReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31081.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
