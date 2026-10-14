namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal license seat assignment policy.</summary>
public static class SeasonalLicenseSeatAssignmentPolicy
{
    public static string SeasonalLicenseSeatAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 61693.0, in order.</summary>
    public static int SeasonalLicenseSeatAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61693.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
