namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pay band audit is in its life cycle.</summary>
public enum StrictPayBandAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One strict pay band audit, with what the planners need to act on it.</summary>
public sealed class StrictPayBandAudit(string code, string label, bool archived, string badge, string jobCode, string checksum, string source, double amount, StrictPayBandAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Badge { get; set; } = badge;
    public string JobCode { get; set; } = jobCode;
    public string Checksum { get; set; } = checksum;
    public string Source { get; set; } = source;
    public double Amount { get; set; } = amount;
    public StrictPayBandAuditStatus Status { get; set; } = status;

    /// <summary>Starts a strict pay band audit in status created.</summary>
    public static StrictPayBandAudit NewStrictPayBandAudit(string code, string label)
    {
        return new StrictPayBandAudit(code, label, false, "", "", "", "", 0.0, StrictPayBandAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPayBandAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPayBandAuditStatus next)
    {
        switch (Status)
        {
            case StrictPayBandAuditStatus.Created:
                return next == StrictPayBandAuditStatus.Validated || next == StrictPayBandAuditStatus.Retired;
            case StrictPayBandAuditStatus.Validated:
                return next == StrictPayBandAuditStatus.Published || next == StrictPayBandAuditStatus.Retired;
            case StrictPayBandAuditStatus.Published:
                switch (next)
                {
                    case StrictPayBandAuditStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPayBandAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pay band audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public StrictPayBandAudit Copy()
    {
        return new StrictPayBandAudit(Code, Label, Archived, Badge, JobCode, Checksum, Source, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
