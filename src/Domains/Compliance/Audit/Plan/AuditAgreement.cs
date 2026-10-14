namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit agreement is in its life cycle.</summary>
public enum AuditAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One audit agreement, with what the planners need to act on it.</summary>
public sealed class AuditAgreement(string code, string name, int expiresAt, double threshold, string country, double cost, AuditAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Threshold { get; set; } = threshold;
    public string Country { get; set; } = country;
    public double Cost { get; set; } = cost;
    public AuditAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a audit agreement in status proposed.</summary>
    public static AuditAgreement NewAuditAgreement(string code, string name)
    {
        return new AuditAgreement(code, name, 0, 0.0, "", 0.0, AuditAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditAgreementStatus next)
    {
        switch (Status)
        {
            case AuditAgreementStatus.Proposed:
                return next == AuditAgreementStatus.Approved;
            case AuditAgreementStatus.Approved:
                switch (next)
                {
                    case AuditAgreementStatus.Rejected:
                    case AuditAgreementStatus.Withdrawn:
                        return true;
                }
                return false;
            case AuditAgreementStatus.Rejected:
                switch (next)
                {
                    case AuditAgreementStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"audit agreement"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)ExpiresAt + Threshold, 2);
    }

    public AuditAgreement Copy()
    {
        return new AuditAgreement(Code, Name, ExpiresAt, Threshold, Country, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
