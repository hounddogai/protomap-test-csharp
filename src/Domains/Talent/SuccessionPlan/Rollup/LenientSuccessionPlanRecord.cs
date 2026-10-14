namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient succession plan record is in its life cycle.</summary>
public enum LenientSuccessionPlanRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a lenient succession plan record and its current status.</summary>
public sealed class LenientSuccessionPlanRecord(string code, string name, double cost, string department, bool pinned, double threshold, int sequence, double rating, double ceiling, int retries, bool visible, LenientSuccessionPlanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Department { get; set; } = department;
    public bool Pinned { get; set; } = pinned;
    public double Threshold { get; set; } = threshold;
    public int Sequence { get; set; } = sequence;
    public double Rating { get; set; } = rating;
    public double Ceiling { get; set; } = ceiling;
    public int Retries { get; set; } = retries;
    public bool Visible { get; set; } = visible;
    public LenientSuccessionPlanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a lenient succession plan record in status planned.</summary>
    public static LenientSuccessionPlanRecord NewLenientSuccessionPlanRecord(string code, string name)
    {
        return new LenientSuccessionPlanRecord(
            code,
            name,
            0.0,
            "",
            false,
            0.0,
            0,
            0.0,
            0.0,
            0,
            false,
            LenientSuccessionPlanRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSuccessionPlanRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSuccessionPlanRecordStatus next)
    {
        switch (Status)
        {
            case LenientSuccessionPlanRecordStatus.Planned:
                switch (next)
                {
                    case LenientSuccessionPlanRecordStatus.Completed:
                        return true;
                }
                return false;
            case LenientSuccessionPlanRecordStatus.Scheduled:
                return next == LenientSuccessionPlanRecordStatus.Cancelled;
            case LenientSuccessionPlanRecordStatus.InProgress:
                switch (next)
                {
                    case LenientSuccessionPlanRecordStatus.Completed:
                    case LenientSuccessionPlanRecordStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientSuccessionPlanRecordStatus.Completed:
                return next == LenientSuccessionPlanRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientSuccessionPlanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient succession plan record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient succession plan record"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Retries - Ceiling, 2);
    }

    public LenientSuccessionPlanRecord Copy()
    {
        return new LenientSuccessionPlanRecord(
            Code,
            Name,
            Cost,
            Department,
            Pinned,
            Threshold,
            Sequence,
            Rating,
            Ceiling,
            Retries,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
