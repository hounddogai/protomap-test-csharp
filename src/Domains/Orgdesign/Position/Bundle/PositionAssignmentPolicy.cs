namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position assignment policy.</summary>
public static class PositionAssignmentPolicy
{
    public static string PositionAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 86700.0, in order.</summary>
    public static int PositionAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86700.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a position assignment code in its canonical PA- form.</summary>
    public static string NormalizePositionAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
