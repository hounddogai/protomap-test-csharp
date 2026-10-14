namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict attestation estimate is in its life cycle.</summary>
public enum StrictAttestationEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One strict attestation estimate, with what the planners need to act on it.</summary>
public sealed class StrictAttestationEstimate(string code, string name, string badge, string salaryBand, bool approved, string location, int limit, StrictAttestationEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Approved { get; set; } = approved;
    public string Location { get; set; } = location;
    public int Limit { get; set; } = limit;
    public StrictAttestationEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a strict attestation estimate in status requested.</summary>
    public static StrictAttestationEstimate NewStrictAttestationEstimate(string code, string name)
    {
        return new StrictAttestationEstimate(
            code,
            name,
            "",
            "",
            false,
            "",
            0,
            StrictAttestationEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAttestationEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAttestationEstimateStatus next)
    {
        switch (Status)
        {
            case StrictAttestationEstimateStatus.Requested:
                switch (next)
                {
                    case StrictAttestationEstimateStatus.Expired:
                        return true;
                }
                return false;
            case StrictAttestationEstimateStatus.Granted:
                switch (next)
                {
                    case StrictAttestationEstimateStatus.Revoked:
                    case StrictAttestationEstimateStatus.Expired:
                        return true;
                }
                return false;
            case StrictAttestationEstimateStatus.Revoked:
                switch (next)
                {
                    case StrictAttestationEstimateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictAttestationEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict attestation estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public StrictAttestationEstimate Copy()
    {
        return new StrictAttestationEstimate(Code, Name, Badge, SalaryBand, Approved, Location, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
