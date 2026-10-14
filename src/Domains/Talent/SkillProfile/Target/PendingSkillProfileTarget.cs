namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending skill profile target is in its life cycle.</summary>
public enum PendingSkillProfileTargetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pending skill profile target, with what the planners need to act on it.</summary>
public sealed class PendingSkillProfileTarget(string code, string name, int tenureMonths, bool archived, string employeeId, int retries, double floorValue, int expiresAt, double minimum, string timezone, PendingSkillProfileTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Archived { get; set; } = archived;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public double FloorValue { get; set; } = floorValue;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Minimum { get; set; } = minimum;
    public string Timezone { get; set; } = timezone;
    public PendingSkillProfileTargetStatus Status { get; set; } = status;

    /// <summary>Starts a pending skill profile target in status requested.</summary>
    public static PendingSkillProfileTarget NewPendingSkillProfileTarget(string code, string name)
    {
        return new PendingSkillProfileTarget(
            code,
            name,
            0,
            false,
            "",
            0,
            0.0,
            0,
            0.0,
            "",
            PendingSkillProfileTargetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSkillProfileTargetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSkillProfileTargetStatus next)
    {
        switch (Status)
        {
            case PendingSkillProfileTargetStatus.Requested:
                return next == PendingSkillProfileTargetStatus.Expired;
            case PendingSkillProfileTargetStatus.Granted:
                switch (next)
                {
                    case PendingSkillProfileTargetStatus.Expired:
                        return true;
                }
                return false;
            case PendingSkillProfileTargetStatus.Revoked:
                switch (next)
                {
                    case PendingSkillProfileTargetStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingSkillProfileTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending skill profile target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(FloorValue * Minimum, 2);
    }

    public PendingSkillProfileTarget Copy()
    {
        return new PendingSkillProfileTarget(
            Code,
            Name,
            TenureMonths,
            Archived,
            EmployeeId,
            Retries,
            FloorValue,
            ExpiresAt,
            Minimum,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
