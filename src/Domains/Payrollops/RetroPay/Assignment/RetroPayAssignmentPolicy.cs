namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of retro pay assignment policy.</summary>
public static class RetroPayAssignmentPolicy
{
    public static string RetroPayAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double RetroPayAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
