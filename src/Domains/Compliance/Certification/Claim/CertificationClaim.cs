namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification claim is in its life cycle.</summary>
public enum CertificationClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One certification claim, with what the planners need to act on it.</summary>
public sealed class CertificationClaim(string code, string name, string timezone, string channel, string grade, int retries, double floorValue, int tenureMonths, string team, string contactEmail, string costCenter, CertificationClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public string Channel { get; set; } = channel;
    public string Grade { get; set; } = grade;
    public int Retries { get; set; } = retries;
    public double FloorValue { get; set; } = floorValue;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public string CostCenter { get; set; } = costCenter;
    public CertificationClaimStatus Status { get; set; } = status;

    /// <summary>Starts a certification claim in status proposed.</summary>
    public static CertificationClaim NewCertificationClaim(string code, string name)
    {
        return new CertificationClaim(
            code,
            name,
            "",
            "",
            "",
            0,
            0.0,
            0,
            "",
            "",
            "",
            CertificationClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationClaimStatus next)
    {
        switch (Status)
        {
            case CertificationClaimStatus.Proposed:
                return next == CertificationClaimStatus.Approved || next == CertificationClaimStatus.Withdrawn;
            case CertificationClaimStatus.Approved:
                return next == CertificationClaimStatus.Rejected || next == CertificationClaimStatus.Withdrawn;
            case CertificationClaimStatus.Rejected:
                switch (next)
                {
                    case CertificationClaimStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"certification claim"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Retries + (double)TenureMonths, 2);
    }

    public CertificationClaim Copy()
    {
        return new CertificationClaim(
            Code,
            Name,
            Timezone,
            Channel,
            Grade,
            Retries,
            FloorValue,
            TenureMonths,
            Team,
            ContactEmail,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
