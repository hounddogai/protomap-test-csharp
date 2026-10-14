namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling policy override is in its life cycle.</summary>
public enum RollingPolicyOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One rolling policy override, with what the planners need to act on it.</summary>
public sealed class RollingPolicyOverride(string code, string name, string country, int quantity, string segment, int durationMinutes, double budget, string externalRef, string badge, int startsAt, int endsAt, RollingPolicyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int Quantity { get; set; } = quantity;
    public string Segment { get; set; } = segment;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Budget { get; set; } = budget;
    public string ExternalRef { get; set; } = externalRef;
    public string Badge { get; set; } = badge;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public RollingPolicyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a rolling policy override in status new.</summary>
    public static RollingPolicyOverride NewRollingPolicyOverride(string code, string name)
    {
        return new RollingPolicyOverride(
            code,
            name,
            "",
            0,
            "",
            0,
            0.0,
            "",
            "",
            0,
            0,
            RollingPolicyOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPolicyOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPolicyOverrideStatus next)
    {
        switch (Status)
        {
            case RollingPolicyOverrideStatus.New:
                switch (next)
                {
                    case RollingPolicyOverrideStatus.Running:
                    case RollingPolicyOverrideStatus.Failed:
                        return true;
                }
                return false;
            case RollingPolicyOverrideStatus.Queued:
                switch (next)
                {
                    case RollingPolicyOverrideStatus.Done:
                        return true;
                }
                return false;
            case RollingPolicyOverrideStatus.Running:
                return next == RollingPolicyOverrideStatus.Failed;
            case RollingPolicyOverrideStatus.Done:
                switch (next)
                {
                    case RollingPolicyOverrideStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPolicyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling policy override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round((double)StartsAt * (double)DurationMinutes, 2);
    }

    public RollingPolicyOverride Copy()
    {
        return new RollingPolicyOverride(
            Code,
            Name,
            Country,
            Quantity,
            Segment,
            DurationMinutes,
            Budget,
            ExternalRef,
            Badge,
            StartsAt,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
