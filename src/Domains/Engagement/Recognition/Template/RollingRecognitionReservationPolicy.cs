namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling recognition reservation policy.</summary>
public static class RollingRecognitionReservationPolicy
{
    public static int RollingRecognitionReservationSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingRecognitionReservation()
    {
        var policy = Retry.DefaultPolicy("rolling-recognition-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
