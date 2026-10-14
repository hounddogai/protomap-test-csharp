namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request checkpoint policy.</summary>
public static class LeaveRequestCheckpointPolicy
{
    public static double LeaveRequestCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string LeaveRequestCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
