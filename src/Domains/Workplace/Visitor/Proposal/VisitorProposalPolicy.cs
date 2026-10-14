namespace Acme.Workforce.Workplace.Visitor.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of visitor proposal policy.</summary>
public static class VisitorProposalPolicy
{
    public static double VisitorProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a visitor proposal score falls in.</summary>
    public static string ClassifyVisitorProposal(double score)
    {
        if (score < 12.0)
        {
            return "ok";
        }
        if (score < 31.1)
        {
            return "watch";
        }
        if (score < 67.7)
        {
            return "minimal";
        }
        if (score < 78.0)
        {
            return "medium";
        }
        return "elevated";
    }
}
