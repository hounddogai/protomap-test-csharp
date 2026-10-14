namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment rollup is in its life cycle.</summary>
public enum AdjustmentRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adjustment rollup. Its code is unique within a tenant.</summary>
public sealed class AdjustmentRollup(string code, string label, string source, string segment, string location, int limit, double maximum, int leaveDays, AdjustmentRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public int Limit { get; set; } = limit;
    public double Maximum { get; set; } = maximum;
    public int LeaveDays { get; set; } = leaveDays;
    public AdjustmentRollupStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment rollup in status planned.</summary>
    public static AdjustmentRollup NewAdjustmentRollup(string code, string label)
    {
        return new AdjustmentRollup(code, label, "", "", "", 0, 0.0, 0, AdjustmentRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentRollupStatus next)
    {
        switch (Status)
        {
            case AdjustmentRollupStatus.Planned:
                switch (next)
                {
                    case AdjustmentRollupStatus.InProgress:
                    case AdjustmentRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case AdjustmentRollupStatus.Scheduled:
                return next == AdjustmentRollupStatus.Completed;
            case AdjustmentRollupStatus.InProgress:
                return next == AdjustmentRollupStatus.Completed || next == AdjustmentRollupStatus.Cancelled;
            case AdjustmentRollupStatus.Completed:
                return next == AdjustmentRollupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdjustmentRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adjustment rollup"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Limit - (double)LeaveDays, 2);
    }

    public AdjustmentRollup Copy()
    {
        return new AdjustmentRollup(Code, Label, Source, Segment, Location, Limit, Maximum, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
