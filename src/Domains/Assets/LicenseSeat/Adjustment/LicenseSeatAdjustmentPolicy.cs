namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>Functions of license seat adjustment policy.</summary>
public static class LicenseSeatAdjustmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 26721.0, in order.</summary>
    public static int LicenseSeatAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26721.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a license seat adjustment score falls in.</summary>
    public static string ClassifyLicenseSeatAdjustment(double score)
    {
        if (score < 51.5)
        {
            return "minimal";
        }
        if (score < 60.4)
        {
            return "low";
        }
        if (score < 95.2)
        {
            return "ok";
        }
        return "critical";
    }
}
