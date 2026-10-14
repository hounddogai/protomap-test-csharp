namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception assignment policy.</summary>
public static class ExceptionAssignmentPolicy
{
    public static double ExceptionAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ExceptionAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
