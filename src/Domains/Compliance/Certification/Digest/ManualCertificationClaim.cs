namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual certification claim is in its life cycle.</summary>
public enum ManualCertificationClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A manual certification claim. Its code is unique within a tenant.</summary>
public sealed class ManualCertificationClaim(string code, string name, int endsAt, int startsAt, string externalRef, ManualCertificationClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public int StartsAt { get; set; } = startsAt;
    public string ExternalRef { get; set; } = externalRef;
    public ManualCertificationClaimStatus Status { get; set; } = status;

    /// <summary>Starts a manual certification claim in status planned.</summary>
    public static ManualCertificationClaim NewManualCertificationClaim(string code, string name)
    {
        return new ManualCertificationClaim(code, name, 0, 0, "", ManualCertificationClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualCertificationClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualCertificationClaimStatus next)
    {
        switch (Status)
        {
            case ManualCertificationClaimStatus.Planned:
                switch (next)
                {
                    case ManualCertificationClaimStatus.InProgress:
                        return true;
                }
                return false;
            case ManualCertificationClaimStatus.Scheduled:
                switch (next)
                {
                    case ManualCertificationClaimStatus.Completed:
                        return true;
                }
                return false;
            case ManualCertificationClaimStatus.InProgress:
                return next == ManualCertificationClaimStatus.Completed || next == ManualCertificationClaimStatus.Cancelled;
            case ManualCertificationClaimStatus.Completed:
                return next == ManualCertificationClaimStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ManualCertificationClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual certification claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"manual certification claim"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt - (double)StartsAt, 2);
    }

    public ManualCertificationClaim Copy()
    {
        return new ManualCertificationClaim(Code, Name, EndsAt, StartsAt, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
