namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility allocation is in its life cycle.</summary>
public enum MobilityAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One mobility allocation, with what the planners need to act on it.</summary>
public sealed class MobilityAllocation(string code, string name, int capacity, double margin, int limit, int updatedAt, MobilityAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Margin { get; set; } = margin;
    public int Limit { get; set; } = limit;
    public int UpdatedAt { get; set; } = updatedAt;
    public MobilityAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a mobility allocation in status new.</summary>
    public static MobilityAllocation NewMobilityAllocation(string code, string name)
    {
        return new MobilityAllocation(code, name, 0, 0.0, 0, 0, MobilityAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityAllocationStatus next)
    {
        switch (Status)
        {
            case MobilityAllocationStatus.New:
                return next == MobilityAllocationStatus.Running;
            case MobilityAllocationStatus.Queued:
                return next == MobilityAllocationStatus.Done || next == MobilityAllocationStatus.Failed;
            case MobilityAllocationStatus.Running:
                switch (next)
                {
                    case MobilityAllocationStatus.Done:
                    case MobilityAllocationStatus.Failed:
                        return true;
                }
                return false;
            case MobilityAllocationStatus.Done:
                return next == MobilityAllocationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(MobilityAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt + (double)Limit, 2);
    }

    public MobilityAllocation Copy()
    {
        return new MobilityAllocation(Code, Name, Capacity, Margin, Limit, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
