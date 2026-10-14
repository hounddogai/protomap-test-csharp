namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary policy override is in its life cycle.</summary>
public enum PrimaryPolicyOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One primary policy override, with what the planners need to act on it.</summary>
public sealed class PrimaryPolicyOverride(string code, string name, int capacity, double rating, int startsAt, int batchSize, string employeeId, string region, PrimaryPolicyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Rating { get; set; } = rating;
    public int StartsAt { get; set; } = startsAt;
    public int BatchSize { get; set; } = batchSize;
    public string EmployeeId { get; set; } = employeeId;
    public string Region { get; set; } = region;
    public PrimaryPolicyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a primary policy override in status proposed.</summary>
    public static PrimaryPolicyOverride NewPrimaryPolicyOverride(string code, string name)
    {
        return new PrimaryPolicyOverride(code, name, 0, 0.0, 0, 0, "", "", PrimaryPolicyOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPolicyOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPolicyOverrideStatus next)
    {
        switch (Status)
        {
            case PrimaryPolicyOverrideStatus.Proposed:
                return next == PrimaryPolicyOverrideStatus.Withdrawn;
            case PrimaryPolicyOverrideStatus.Approved:
                switch (next)
                {
                    case PrimaryPolicyOverrideStatus.Rejected:
                        return true;
                }
                return false;
            case PrimaryPolicyOverrideStatus.Rejected:
                switch (next)
                {
                    case PrimaryPolicyOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryPolicyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary policy override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)BatchSize * Rating, 2);
    }

    public PrimaryPolicyOverride Copy()
    {
        return new PrimaryPolicyOverride(
            Code,
            Name,
            Capacity,
            Rating,
            StartsAt,
            BatchSize,
            EmployeeId,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
