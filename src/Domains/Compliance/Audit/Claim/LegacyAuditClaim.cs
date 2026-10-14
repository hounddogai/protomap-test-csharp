namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy audit claim is in its life cycle.</summary>
public enum LegacyAuditClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a legacy audit claim and its current status.</summary>
public sealed class LegacyAuditClaim(string code, string title, int revision, string salaryBand, string ownerId, bool archived, int updatedAt, LegacyAuditClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string SalaryBand { get; set; } = salaryBand;
    public string OwnerId { get; set; } = ownerId;
    public bool Archived { get; set; } = archived;
    public int UpdatedAt { get; set; } = updatedAt;
    public LegacyAuditClaimStatus Status { get; set; } = status;

    /// <summary>Starts a legacy audit claim in status proposed.</summary>
    public static LegacyAuditClaim NewLegacyAuditClaim(string code, string title)
    {
        return new LegacyAuditClaim(code, title, 0, "", "", false, 0, LegacyAuditClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAuditClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAuditClaimStatus next)
    {
        switch (Status)
        {
            case LegacyAuditClaimStatus.Proposed:
                return next == LegacyAuditClaimStatus.Approved || next == LegacyAuditClaimStatus.Withdrawn;
            case LegacyAuditClaimStatus.Approved:
                switch (next)
                {
                    case LegacyAuditClaimStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacyAuditClaimStatus.Rejected:
                switch (next)
                {
                    case LegacyAuditClaimStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAuditClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy audit claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"legacy audit claim"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Revision + (double)UpdatedAt, 2);
    }

    public LegacyAuditClaim Copy()
    {
        return new LegacyAuditClaim(Code, Title, Revision, SalaryBand, OwnerId, Archived, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
