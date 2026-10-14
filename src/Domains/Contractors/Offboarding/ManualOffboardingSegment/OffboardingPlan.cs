namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding plan is in its life cycle.</summary>
public enum OffboardingPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A offboarding plan. Its code is unique within a tenant.</summary>
public sealed class OffboardingPlan(string code, string label, string description, int expiresAt, double fte, int capacity, double floorValue, bool active, bool archived, int durationMinutes, OffboardingPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Fte { get; set; } = fte;
    public int Capacity { get; set; } = capacity;
    public double FloorValue { get; set; } = floorValue;
    public bool Active { get; set; } = active;
    public bool Archived { get; set; } = archived;
    public int DurationMinutes { get; set; } = durationMinutes;
    public OffboardingPlanStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding plan in status requested.</summary>
    public static OffboardingPlan NewOffboardingPlan(string code, string label)
    {
        return new OffboardingPlan(
            code,
            label,
            "",
            0,
            0.0,
            0,
            0.0,
            false,
            false,
            0,
            OffboardingPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingPlanStatus next)
    {
        switch (Status)
        {
            case OffboardingPlanStatus.Requested:
                return next == OffboardingPlanStatus.Expired;
            case OffboardingPlanStatus.Granted:
                switch (next)
                {
                    case OffboardingPlanStatus.Revoked:
                        return true;
                }
                return false;
            case OffboardingPlanStatus.Revoked:
                switch (next)
                {
                    case OffboardingPlanStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt * Fte, 2);
    }

    public OffboardingPlan Copy()
    {
        return new OffboardingPlan(
            Code,
            Label,
            Description,
            ExpiresAt,
            Fte,
            Capacity,
            FloorValue,
            Active,
            Archived,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
