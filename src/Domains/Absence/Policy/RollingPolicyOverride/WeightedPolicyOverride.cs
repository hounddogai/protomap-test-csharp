namespace Acme.Workforce.Absence.Policy.RollingPolicyOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted policy override is in its life cycle.</summary>
public enum WeightedPolicyOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A weighted policy override as the policy team keeps it.</summary>
public sealed class WeightedPolicyOverride(string code, string name, string timezone, int version, int batchSize, double hours, string externalRef, double score, int limit, int startsAt, double weight, WeightedPolicyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public double Hours { get; set; } = hours;
    public string ExternalRef { get; set; } = externalRef;
    public double Score { get; set; } = score;
    public int Limit { get; set; } = limit;
    public int StartsAt { get; set; } = startsAt;
    public double Weight { get; set; } = weight;
    public WeightedPolicyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a weighted policy override in status open.</summary>
    public static WeightedPolicyOverride NewWeightedPolicyOverride(string code, string name)
    {
        return new WeightedPolicyOverride(
            code,
            name,
            "",
            0,
            0,
            0.0,
            "",
            0.0,
            0,
            0,
            0.0,
            WeightedPolicyOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPolicyOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPolicyOverrideStatus next)
    {
        switch (Status)
        {
            case WeightedPolicyOverrideStatus.Open:
                switch (next)
                {
                    case WeightedPolicyOverrideStatus.Archived:
                        return true;
                }
                return false;
            case WeightedPolicyOverrideStatus.InReview:
                switch (next)
                {
                    case WeightedPolicyOverrideStatus.Reopened:
                    case WeightedPolicyOverrideStatus.Archived:
                        return true;
                }
                return false;
            case WeightedPolicyOverrideStatus.Resolved:
                switch (next)
                {
                    case WeightedPolicyOverrideStatus.Reopened:
                    case WeightedPolicyOverrideStatus.Archived:
                        return true;
                }
                return false;
            case WeightedPolicyOverrideStatus.Reopened:
                return next == WeightedPolicyOverrideStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedPolicyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted policy override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Limit - Weight, 2);
    }

    public WeightedPolicyOverride Copy()
    {
        return new WeightedPolicyOverride(
            Code,
            Name,
            Timezone,
            Version,
            BatchSize,
            Hours,
            ExternalRef,
            Score,
            Limit,
            StartsAt,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
