namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat proposal policy.</summary>
public static class LicenseSeatProposalPolicy
{
    public static int LicenseSeatProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double LicenseSeatProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.13;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
