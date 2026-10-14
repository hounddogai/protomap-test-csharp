namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary pool override is in its life cycle.</summary>
public enum PrimaryPoolOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One primary pool override, with what the planners need to act on it.</summary>
public sealed class PrimaryPoolOverride(string code, string name, string salaryBand, int expiresAt, string checksum, string employeeId, int durationMinutes, PrimaryPoolOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Checksum { get; set; } = checksum;
    public string EmployeeId { get; set; } = employeeId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public PrimaryPoolOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a primary pool override in status requested.</summary>
    public static PrimaryPoolOverride NewPrimaryPoolOverride(string code, string name)
    {
        return new PrimaryPoolOverride(code, name, "", 0, "", "", 0, PrimaryPoolOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPoolOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPoolOverrideStatus next)
    {
        switch (Status)
        {
            case PrimaryPoolOverrideStatus.Requested:
                return next == PrimaryPoolOverrideStatus.Revoked;
            case PrimaryPoolOverrideStatus.Granted:
                switch (next)
                {
                    case PrimaryPoolOverrideStatus.Revoked:
                    case PrimaryPoolOverrideStatus.Expired:
                        return true;
                }
                return false;
            case PrimaryPoolOverrideStatus.Revoked:
                return next == PrimaryPoolOverrideStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PrimaryPoolOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary pool override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary pool override"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)ExpiresAt * (double)DurationMinutes, 2);
    }

    public PrimaryPoolOverride Copy()
    {
        return new PrimaryPoolOverride(
            Code,
            Name,
            SalaryBand,
            ExpiresAt,
            Checksum,
            EmployeeId,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
