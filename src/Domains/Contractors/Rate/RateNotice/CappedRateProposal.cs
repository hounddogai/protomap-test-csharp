namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped rate proposal is in its life cycle.</summary>
public enum CappedRateProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A capped rate proposal as the rate team keeps it.</summary>
public sealed class CappedRateProposal(string code, string name, string contactEmail, int durationMinutes, int endsAt, int limit, bool active, double margin, int priority, CappedRateProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int EndsAt { get; set; } = endsAt;
    public int Limit { get; set; } = limit;
    public bool Active { get; set; } = active;
    public double Margin { get; set; } = margin;
    public int Priority { get; set; } = priority;
    public CappedRateProposalStatus Status { get; set; } = status;

    /// <summary>Starts a capped rate proposal in status created.</summary>
    public static CappedRateProposal NewCappedRateProposal(string code, string name)
    {
        return new CappedRateProposal(code, name, "", 0, 0, 0, false, 0.0, 0, CappedRateProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRateProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRateProposalStatus next)
    {
        switch (Status)
        {
            case CappedRateProposalStatus.Created:
                return next == CappedRateProposalStatus.Published;
            case CappedRateProposalStatus.Validated:
                switch (next)
                {
                    case CappedRateProposalStatus.Published:
                    case CappedRateProposalStatus.Retired:
                        return true;
                }
                return false;
            case CappedRateProposalStatus.Published:
                switch (next)
                {
                    case CappedRateProposalStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedRateProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped rate proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"capped rate proposal"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Priority + (double)Limit, 2);
    }

    public CappedRateProposal Copy()
    {
        return new CappedRateProposal(
            Code,
            Name,
            ContactEmail,
            DurationMinutes,
            EndsAt,
            Limit,
            Active,
            Margin,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
