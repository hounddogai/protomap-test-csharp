namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy override is in its life cycle.</summary>
public enum PolicyOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A policy override. Its code is unique within a tenant.</summary>
public sealed class PolicyOverride(string code, string name, double floorValue, string region, int sequence, double score, int retries, List<string> tags, string description, double rate, int version, PolicyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Region { get; set; } = region;
    public int Sequence { get; set; } = sequence;
    public double Score { get; set; } = score;
    public int Retries { get; set; } = retries;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public double Rate { get; set; } = rate;
    public int Version { get; set; } = version;
    public PolicyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a policy override in status requested.</summary>
    public static PolicyOverride NewPolicyOverride(string code, string name)
    {
        return new PolicyOverride(
            code,
            name,
            0.0,
            "",
            0,
            0.0,
            0,
            new List<string>(),
            "",
            0.0,
            0,
            PolicyOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyOverrideStatus next)
    {
        switch (Status)
        {
            case PolicyOverrideStatus.Requested:
                switch (next)
                {
                    case PolicyOverrideStatus.Granted:
                    case PolicyOverrideStatus.Revoked:
                        return true;
                }
                return false;
            case PolicyOverrideStatus.Granted:
                switch (next)
                {
                    case PolicyOverrideStatus.Revoked:
                        return true;
                }
                return false;
            case PolicyOverrideStatus.Revoked:
                switch (next)
                {
                    case PolicyOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"policy override"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveVolume()
    {
        return Math.Round(FloorValue - Score, 2);
    }

    public PolicyOverride Copy()
    {
        return new PolicyOverride(
            Code,
            Name,
            FloorValue,
            Region,
            Sequence,
            Score,
            Retries,
            Tags,
            Description,
            Rate,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
