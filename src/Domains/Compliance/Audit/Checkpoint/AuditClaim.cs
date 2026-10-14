namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit claim is in its life cycle.</summary>
public enum AuditClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A audit claim as the audit team keeps it.</summary>
public sealed class AuditClaim(string code, string name, int retries, int startsAt, int updatedAt, AuditClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public int StartsAt { get; set; } = startsAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public AuditClaimStatus Status { get; set; } = status;

    /// <summary>Starts a audit claim in status idle.</summary>
    public static AuditClaim NewAuditClaim(string code, string name)
    {
        return new AuditClaim(code, name, 0, 0, 0, AuditClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditClaimStatus next)
    {
        switch (Status)
        {
            case AuditClaimStatus.Idle:
                return next == AuditClaimStatus.Stopped;
            case AuditClaimStatus.Warming:
                switch (next)
                {
                    case AuditClaimStatus.Draining:
                        return true;
                }
                return false;
            case AuditClaimStatus.Ready:
                return next == AuditClaimStatus.Draining || next == AuditClaimStatus.Stopped;
            case AuditClaimStatus.Draining:
                switch (next)
                {
                    case AuditClaimStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)UpdatedAt * (double)StartsAt, 2);
    }

    public AuditClaim Copy()
    {
        return new AuditClaim(Code, Name, Retries, StartsAt, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
