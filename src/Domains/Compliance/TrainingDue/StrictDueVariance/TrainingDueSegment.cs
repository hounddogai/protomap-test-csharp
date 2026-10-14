namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due segment is in its life cycle.</summary>
public enum TrainingDueSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a training due segment and its current status.</summary>
public sealed class TrainingDueSegment(string code, string name, string country, double floorValue, int windowDays, double discount, string segment, int priority, bool visible, TrainingDueSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double FloorValue { get; set; } = floorValue;
    public int WindowDays { get; set; } = windowDays;
    public double Discount { get; set; } = discount;
    public string Segment { get; set; } = segment;
    public int Priority { get; set; } = priority;
    public bool Visible { get; set; } = visible;
    public TrainingDueSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a training due segment in status proposed.</summary>
    public static TrainingDueSegment NewTrainingDueSegment(string code, string name)
    {
        return new TrainingDueSegment(code, name, "", 0.0, 0, 0.0, "", 0, false, TrainingDueSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueSegmentStatus next)
    {
        switch (Status)
        {
            case TrainingDueSegmentStatus.Proposed:
                switch (next)
                {
                    case TrainingDueSegmentStatus.Rejected:
                    case TrainingDueSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case TrainingDueSegmentStatus.Approved:
                switch (next)
                {
                    case TrainingDueSegmentStatus.Rejected:
                    case TrainingDueSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case TrainingDueSegmentStatus.Rejected:
                return next == TrainingDueSegmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TrainingDueSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"training due segment"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Discount + (double)WindowDays, 2);
    }

    public TrainingDueSegment Copy()
    {
        return new TrainingDueSegment(
            Code,
            Name,
            Country,
            FloorValue,
            WindowDays,
            Discount,
            Segment,
            Priority,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
