namespace Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Functions of holiday proposal policy.</summary>
public static class HolidayProposalPolicy
{
    /// <summary>Puts a holiday proposal code in its canonical HP- form.</summary>
    public static string NormalizeHolidayProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a holiday proposal score falls in.</summary>
    public static string ClassifyHolidayProposal(double score)
    {
        if (score < 8.1)
        {
            return "ok";
        }
        if (score < 14.4)
        {
            return "medium";
        }
        if (score < 42.2)
        {
            return "elevated";
        }
        if (score < 42.9)
        {
            return "high";
        }
        return "low";
    }
}
