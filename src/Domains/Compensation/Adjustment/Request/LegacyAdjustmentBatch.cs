namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy adjustment batch is in its life cycle.</summary>
public enum LegacyAdjustmentBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy adjustment batch. Its code is unique within a tenant.</summary>
public sealed class LegacyAdjustmentBatch(string code, string title, string team, string unit, string description, string location, string summary, double ceiling, LegacyAdjustmentBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public string Location { get; set; } = location;
    public string Summary { get; set; } = summary;
    public double Ceiling { get; set; } = ceiling;
    public LegacyAdjustmentBatchStatus Status { get; set; } = status;

    /// <summary>Starts a legacy adjustment batch in status planned.</summary>
    public static LegacyAdjustmentBatch NewLegacyAdjustmentBatch(string code, string title)
    {
        return new LegacyAdjustmentBatch(code, title, "", "", "", "", "", 0.0, LegacyAdjustmentBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAdjustmentBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAdjustmentBatchStatus next)
    {
        switch (Status)
        {
            case LegacyAdjustmentBatchStatus.Planned:
                switch (next)
                {
                    case LegacyAdjustmentBatchStatus.Scheduled:
                    case LegacyAdjustmentBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyAdjustmentBatchStatus.Scheduled:
                return next == LegacyAdjustmentBatchStatus.InProgress || next == LegacyAdjustmentBatchStatus.Completed;
            case LegacyAdjustmentBatchStatus.InProgress:
                switch (next)
                {
                    case LegacyAdjustmentBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyAdjustmentBatchStatus.Completed:
                switch (next)
                {
                    case LegacyAdjustmentBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAdjustmentBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy adjustment batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public LegacyAdjustmentBatch Copy()
    {
        return new LegacyAdjustmentBatch(Code, Title, Team, Unit, Description, Location, Summary, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
