namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh allocation is in its life cycle.</summary>
public enum RefreshAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a refresh allocation and its current status.</summary>
public sealed class RefreshAllocation(string code, string name, Dictionary<string, string> attributes, int startsAt, bool active, int version, RefreshAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int StartsAt { get; set; } = startsAt;
    public bool Active { get; set; } = active;
    public int Version { get; set; } = version;
    public RefreshAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a refresh allocation in status idle.</summary>
    public static RefreshAllocation NewRefreshAllocation(string code, string name)
    {
        return new RefreshAllocation(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            false,
            0,
            RefreshAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshAllocationStatus next)
    {
        switch (Status)
        {
            case RefreshAllocationStatus.Idle:
                return next == RefreshAllocationStatus.Warming;
            case RefreshAllocationStatus.Warming:
                switch (next)
                {
                    case RefreshAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case RefreshAllocationStatus.Ready:
                return next == RefreshAllocationStatus.Stopped;
            case RefreshAllocationStatus.Draining:
                return next == RefreshAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RefreshAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Version * (double)StartsAt, 2);
    }

    public RefreshAllocation Copy()
    {
        return new RefreshAllocation(Code, Name, Attributes, StartsAt, Active, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
