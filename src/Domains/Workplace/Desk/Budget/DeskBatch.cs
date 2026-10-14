namespace Acme.Workforce.Workplace.Desk.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk batch is in its life cycle.</summary>
public enum DeskBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One desk batch, with what the planners need to act on it.</summary>
public sealed class DeskBatch(string code, string name, bool archived, string unit, string location, int leaveDays, string badge, double maximum, double cost, int revision, DeskBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Unit { get; set; } = unit;
    public string Location { get; set; } = location;
    public int LeaveDays { get; set; } = leaveDays;
    public string Badge { get; set; } = badge;
    public double Maximum { get; set; } = maximum;
    public double Cost { get; set; } = cost;
    public int Revision { get; set; } = revision;
    public DeskBatchStatus Status { get; set; } = status;

    /// <summary>Starts a desk batch in status idle.</summary>
    public static DeskBatch NewDeskBatch(string code, string name)
    {
        return new DeskBatch(code, name, false, "", "", 0, "", 0.0, 0.0, 0, DeskBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskBatchStatus next)
    {
        switch (Status)
        {
            case DeskBatchStatus.Idle:
                return next == DeskBatchStatus.Warming || next == DeskBatchStatus.Stopped;
            case DeskBatchStatus.Warming:
                return next == DeskBatchStatus.Draining || next == DeskBatchStatus.Stopped;
            case DeskBatchStatus.Ready:
                switch (next)
                {
                    case DeskBatchStatus.Stopped:
                        return true;
                }
                return false;
            case DeskBatchStatus.Draining:
                return next == DeskBatchStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DeskBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Maximum + Cost, 2);
    }

    public DeskBatch Copy()
    {
        return new DeskBatch(
            Code,
            Name,
            Archived,
            Unit,
            Location,
            LeaveDays,
            Badge,
            Maximum,
            Cost,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
