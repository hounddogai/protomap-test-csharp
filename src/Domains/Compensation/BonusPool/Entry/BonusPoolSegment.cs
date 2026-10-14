namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool segment is in its life cycle.</summary>
public enum BonusPoolSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One bonus pool segment, with what the planners need to act on it.</summary>
public sealed class BonusPoolSegment(string code, string label, int priority, double maximum, string country, double fte, int version, int batchSize, BonusPoolSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public double Maximum { get; set; } = maximum;
    public string Country { get; set; } = country;
    public double Fte { get; set; } = fte;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public BonusPoolSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool segment in status new.</summary>
    public static BonusPoolSegment NewBonusPoolSegment(string code, string label)
    {
        return new BonusPoolSegment(code, label, 0, 0.0, "", 0.0, 0, 0, BonusPoolSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolSegmentStatus next)
    {
        switch (Status)
        {
            case BonusPoolSegmentStatus.New:
                return next == BonusPoolSegmentStatus.Queued;
            case BonusPoolSegmentStatus.Queued:
                switch (next)
                {
                    case BonusPoolSegmentStatus.Running:
                    case BonusPoolSegmentStatus.Done:
                        return true;
                }
                return false;
            case BonusPoolSegmentStatus.Running:
                switch (next)
                {
                    case BonusPoolSegmentStatus.Done:
                        return true;
                }
                return false;
            case BonusPoolSegmentStatus.Done:
                return next == BonusPoolSegmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BonusPoolSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool segment"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Version - Fte, 2);
    }

    public BonusPoolSegment Copy()
    {
        return new BonusPoolSegment(Code, Label, Priority, Maximum, Country, Fte, Version, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
