namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal audit claim is in its life cycle.</summary>
public enum SeasonalAuditClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a seasonal audit claim and its current status.</summary>
public sealed class SeasonalAuditClaim(string code, string label, double amount, string country, bool visible, string jobCode, SeasonalAuditClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string Country { get; set; } = country;
    public bool Visible { get; set; } = visible;
    public string JobCode { get; set; } = jobCode;
    public SeasonalAuditClaimStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal audit claim in status draft.</summary>
    public static SeasonalAuditClaim NewSeasonalAuditClaim(string code, string label)
    {
        return new SeasonalAuditClaim(code, label, 0.0, "", false, "", SeasonalAuditClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAuditClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAuditClaimStatus next)
    {
        switch (Status)
        {
            case SeasonalAuditClaimStatus.Draft:
                return next == SeasonalAuditClaimStatus.Suspended;
            case SeasonalAuditClaimStatus.Pending:
                switch (next)
                {
                    case SeasonalAuditClaimStatus.Active:
                    case SeasonalAuditClaimStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalAuditClaimStatus.Active:
                switch (next)
                {
                    case SeasonalAuditClaimStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalAuditClaimStatus.Suspended:
                switch (next)
                {
                    case SeasonalAuditClaimStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAuditClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal audit claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"seasonal audit claim"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public SeasonalAuditClaim Copy()
    {
        return new SeasonalAuditClaim(Code, Label, Amount, Country, Visible, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
