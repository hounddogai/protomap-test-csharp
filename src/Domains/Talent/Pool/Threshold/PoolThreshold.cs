namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool threshold is in its life cycle.</summary>
public enum PoolThresholdStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pool threshold, with what the planners need to act on it.</summary>
public sealed class PoolThreshold(string code, string name, string department, string skill, string team, string region, string phone, string segment, int expiresAt, PoolThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string Skill { get; set; } = skill;
    public string Team { get; set; } = team;
    public string Region { get; set; } = region;
    public string Phone { get; set; } = phone;
    public string Segment { get; set; } = segment;
    public int ExpiresAt { get; set; } = expiresAt;
    public PoolThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a pool threshold in status created.</summary>
    public static PoolThreshold NewPoolThreshold(string code, string name)
    {
        return new PoolThreshold(code, name, "", "", "", "", "", "", 0, PoolThresholdStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolThresholdStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolThresholdStatus next)
    {
        switch (Status)
        {
            case PoolThresholdStatus.Created:
                return next == PoolThresholdStatus.Retired;
            case PoolThresholdStatus.Validated:
                switch (next)
                {
                    case PoolThresholdStatus.Published:
                    case PoolThresholdStatus.Retired:
                        return true;
                }
                return false;
            case PoolThresholdStatus.Published:
                return next == PoolThresholdStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PoolThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public PoolThreshold Copy()
    {
        return new PoolThreshold(Code, Name, Department, Skill, Team, Region, Phone, Segment, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
