namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy deduction score is in its life cycle.</summary>
public enum LegacyDeductionScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One legacy deduction score, with what the planners need to act on it.</summary>
public sealed class LegacyDeductionScore(string code, string name, int tenureMonths, string badge, int retries, string country, string source, string ownerId, int limit, LegacyDeductionScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Badge { get; set; } = badge;
    public int Retries { get; set; } = retries;
    public string Country { get; set; } = country;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public int Limit { get; set; } = limit;
    public LegacyDeductionScoreStatus Status { get; set; } = status;

    /// <summary>Starts a legacy deduction score in status created.</summary>
    public static LegacyDeductionScore NewLegacyDeductionScore(string code, string name)
    {
        return new LegacyDeductionScore(code, name, 0, "", 0, "", "", "", 0, LegacyDeductionScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyDeductionScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyDeductionScoreStatus next)
    {
        switch (Status)
        {
            case LegacyDeductionScoreStatus.Created:
                switch (next)
                {
                    case LegacyDeductionScoreStatus.Validated:
                        return true;
                }
                return false;
            case LegacyDeductionScoreStatus.Validated:
                return next == LegacyDeductionScoreStatus.Published || next == LegacyDeductionScoreStatus.Retired;
            case LegacyDeductionScoreStatus.Published:
                return next == LegacyDeductionScoreStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LegacyDeductionScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy deduction score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Retries * (double)Limit, 2);
    }

    public LegacyDeductionScore Copy()
    {
        return new LegacyDeductionScore(
            Code,
            Name,
            TenureMonths,
            Badge,
            Retries,
            Country,
            Source,
            OwnerId,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
