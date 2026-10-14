namespace Acme.Workforce.Compliance.Attestation.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation audit is in its life cycle.</summary>
public enum AttestationAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A attestation audit. Its code is unique within a tenant.</summary>
public sealed class AttestationAudit(string code, string title, string timezone, int durationMinutes, double rate, double cost, double budget, string costCenter, string externalRef, AttestationAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rate { get; set; } = rate;
    public double Cost { get; set; } = cost;
    public double Budget { get; set; } = budget;
    public string CostCenter { get; set; } = costCenter;
    public string ExternalRef { get; set; } = externalRef;
    public AttestationAuditStatus Status { get; set; } = status;

    /// <summary>Starts a attestation audit in status idle.</summary>
    public static AttestationAudit NewAttestationAudit(string code, string title)
    {
        return new AttestationAudit(code, title, "", 0, 0.0, 0.0, 0.0, "", "", AttestationAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationAuditStatus next)
    {
        switch (Status)
        {
            case AttestationAuditStatus.Idle:
                return next == AttestationAuditStatus.Warming;
            case AttestationAuditStatus.Warming:
                return next == AttestationAuditStatus.Draining;
            case AttestationAuditStatus.Ready:
                return next == AttestationAuditStatus.Stopped;
            case AttestationAuditStatus.Draining:
                switch (next)
                {
                    case AttestationAuditStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Cost + Budget, 2);
    }

    public AttestationAudit Copy()
    {
        return new AttestationAudit(
            Code,
            Title,
            Timezone,
            DurationMinutes,
            Rate,
            Cost,
            Budget,
            CostCenter,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
