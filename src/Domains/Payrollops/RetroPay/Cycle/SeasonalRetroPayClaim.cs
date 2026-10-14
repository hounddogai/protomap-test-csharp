namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal retro pay claim is in its life cycle.</summary>
public enum SeasonalRetroPayClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One seasonal retro pay claim, with what the planners need to act on it.</summary>
public sealed class SeasonalRetroPayClaim(string code, string name, int updatedAt, string title, string jobCode, int limit, string label, string skill, double budget, string notes, SeasonalRetroPayClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public int Limit { get; set; } = limit;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public double Budget { get; set; } = budget;
    public string Notes { get; set; } = notes;
    public SeasonalRetroPayClaimStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal retro pay claim in status created.</summary>
    public static SeasonalRetroPayClaim NewSeasonalRetroPayClaim(string code, string name)
    {
        return new SeasonalRetroPayClaim(
            code,
            name,
            0,
            "",
            "",
            0,
            "",
            "",
            0.0,
            "",
            SeasonalRetroPayClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalRetroPayClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalRetroPayClaimStatus next)
    {
        switch (Status)
        {
            case SeasonalRetroPayClaimStatus.Created:
                switch (next)
                {
                    case SeasonalRetroPayClaimStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalRetroPayClaimStatus.Validated:
                return next == SeasonalRetroPayClaimStatus.Published;
            case SeasonalRetroPayClaimStatus.Published:
                switch (next)
                {
                    case SeasonalRetroPayClaimStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalRetroPayClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal retro pay claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double NetLoad()
    {
        return Math.Round(Budget + (double)Limit, 2);
    }

    public SeasonalRetroPayClaim Copy()
    {
        return new SeasonalRetroPayClaim(
            Code,
            Name,
            UpdatedAt,
            Title,
            JobCode,
            Limit,
            Label,
            Skill,
            Budget,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
