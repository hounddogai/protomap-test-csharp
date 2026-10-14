namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model threshold is in its life cycle.</summary>
public enum CostModelThresholdStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A cost model threshold as the cost model team keeps it.</summary>
public sealed class CostModelThreshold(string code, string name, int sequence, string region, int priority, string notes, double ceiling, CostModelThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public string Region { get; set; } = region;
    public int Priority { get; set; } = priority;
    public string Notes { get; set; } = notes;
    public double Ceiling { get; set; } = ceiling;
    public CostModelThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a cost model threshold in status planned.</summary>
    public static CostModelThreshold NewCostModelThreshold(string code, string name)
    {
        return new CostModelThreshold(code, name, 0, "", 0, "", 0.0, CostModelThresholdStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelThresholdStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelThresholdStatus next)
    {
        switch (Status)
        {
            case CostModelThresholdStatus.Planned:
                switch (next)
                {
                    case CostModelThresholdStatus.Cancelled:
                        return true;
                }
                return false;
            case CostModelThresholdStatus.Scheduled:
                return next == CostModelThresholdStatus.Completed;
            case CostModelThresholdStatus.InProgress:
                switch (next)
                {
                    case CostModelThresholdStatus.Completed:
                    case CostModelThresholdStatus.Cancelled:
                        return true;
                }
                return false;
            case CostModelThresholdStatus.Completed:
                switch (next)
                {
                    case CostModelThresholdStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Ceiling - (double)Priority, 2);
    }

    public CostModelThreshold Copy()
    {
        return new CostModelThreshold(Code, Name, Sequence, Region, Priority, Notes, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
