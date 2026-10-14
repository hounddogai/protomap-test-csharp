namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict cost model checkpoint is in its life cycle.</summary>
public enum StrictCostModelCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A strict cost model checkpoint. Its code is unique within a tenant.</summary>
public sealed class StrictCostModelCheckpoint(string code, string label, double minimum, double rate, int tenureMonths, string unit, double ceiling, string location, int sequence, string channel, bool visible, StrictCostModelCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public double Rate { get; set; } = rate;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public string Location { get; set; } = location;
    public int Sequence { get; set; } = sequence;
    public string Channel { get; set; } = channel;
    public bool Visible { get; set; } = visible;
    public StrictCostModelCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a strict cost model checkpoint in status open.</summary>
    public static StrictCostModelCheckpoint NewStrictCostModelCheckpoint(string code, string label)
    {
        return new StrictCostModelCheckpoint(
            code,
            label,
            0.0,
            0.0,
            0,
            "",
            0.0,
            "",
            0,
            "",
            false,
            StrictCostModelCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictCostModelCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictCostModelCheckpointStatus next)
    {
        switch (Status)
        {
            case StrictCostModelCheckpointStatus.Open:
                return next == StrictCostModelCheckpointStatus.Reopened || next == StrictCostModelCheckpointStatus.Archived;
            case StrictCostModelCheckpointStatus.InReview:
                return next == StrictCostModelCheckpointStatus.Resolved;
            case StrictCostModelCheckpointStatus.Resolved:
                return next == StrictCostModelCheckpointStatus.Reopened || next == StrictCostModelCheckpointStatus.Archived;
            case StrictCostModelCheckpointStatus.Reopened:
                return next == StrictCostModelCheckpointStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictCostModelCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict cost model checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict cost model checkpoint"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Rate * (double)TenureMonths, 2);
    }

    public StrictCostModelCheckpoint Copy()
    {
        return new StrictCostModelCheckpoint(
            Code,
            Label,
            Minimum,
            Rate,
            TenureMonths,
            Unit,
            Ceiling,
            Location,
            Sequence,
            Channel,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
