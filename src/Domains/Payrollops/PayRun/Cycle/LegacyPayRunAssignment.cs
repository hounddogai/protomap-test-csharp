namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay run assignment is in its life cycle.</summary>
public enum LegacyPayRunAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A legacy pay run assignment as the pay run team keeps it.</summary>
public sealed class LegacyPayRunAssignment(string code, string name, string label, int priority, int revision, LegacyPayRunAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public int Revision { get; set; } = revision;
    public LegacyPayRunAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay run assignment in status idle.</summary>
    public static LegacyPayRunAssignment NewLegacyPayRunAssignment(string code, string name)
    {
        return new LegacyPayRunAssignment(code, name, "", 0, 0, LegacyPayRunAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayRunAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayRunAssignmentStatus next)
    {
        switch (Status)
        {
            case LegacyPayRunAssignmentStatus.Idle:
                return next == LegacyPayRunAssignmentStatus.Stopped;
            case LegacyPayRunAssignmentStatus.Warming:
                return next == LegacyPayRunAssignmentStatus.Ready || next == LegacyPayRunAssignmentStatus.Draining;
            case LegacyPayRunAssignmentStatus.Ready:
                switch (next)
                {
                    case LegacyPayRunAssignmentStatus.Draining:
                    case LegacyPayRunAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyPayRunAssignmentStatus.Draining:
                switch (next)
                {
                    case LegacyPayRunAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPayRunAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay run assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"legacy pay run assignment"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Priority * (double)Revision, 2);
    }

    public LegacyPayRunAssignment Copy()
    {
        return new LegacyPayRunAssignment(Code, Name, Label, Priority, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
