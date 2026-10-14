namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg allocation is in its life cycle.</summary>
public enum ReorgAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A reorg allocation. Its code is unique within a tenant.</summary>
public sealed class ReorgAllocation(string code, string name, double threshold, string managerId, string skill, int version, ReorgAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string ManagerId { get; set; } = managerId;
    public string Skill { get; set; } = skill;
    public int Version { get; set; } = version;
    public ReorgAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a reorg allocation in status idle.</summary>
    public static ReorgAllocation NewReorgAllocation(string code, string name)
    {
        return new ReorgAllocation(code, name, 0.0, "", "", 0, ReorgAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgAllocationStatus next)
    {
        switch (Status)
        {
            case ReorgAllocationStatus.Idle:
                return next == ReorgAllocationStatus.Draining || next == ReorgAllocationStatus.Stopped;
            case ReorgAllocationStatus.Warming:
                return next == ReorgAllocationStatus.Ready || next == ReorgAllocationStatus.Draining;
            case ReorgAllocationStatus.Ready:
                return next == ReorgAllocationStatus.Draining || next == ReorgAllocationStatus.Stopped;
            case ReorgAllocationStatus.Draining:
                return next == ReorgAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ReorgAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Version * Threshold, 2);
    }

    public ReorgAllocation Copy()
    {
        return new ReorgAllocation(Code, Name, Threshold, ManagerId, Skill, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
