namespace Acme.Workforce.Engagement.TownHall.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall segment is in its life cycle.</summary>
public enum TownHallSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One town hall segment, with what the planners need to act on it.</summary>
public sealed class TownHallSegment(string code, string name, int retries, int capacity, double threshold, TownHallSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public int Capacity { get; set; } = capacity;
    public double Threshold { get; set; } = threshold;
    public TownHallSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a town hall segment in status draft.</summary>
    public static TownHallSegment NewTownHallSegment(string code, string name)
    {
        return new TownHallSegment(code, name, 0, 0, 0.0, TownHallSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallSegmentStatus next)
    {
        switch (Status)
        {
            case TownHallSegmentStatus.Draft:
                return next == TownHallSegmentStatus.Active;
            case TownHallSegmentStatus.Pending:
                return next == TownHallSegmentStatus.Suspended || next == TownHallSegmentStatus.Closed;
            case TownHallSegmentStatus.Active:
                switch (next)
                {
                    case TownHallSegmentStatus.Suspended:
                    case TownHallSegmentStatus.Closed:
                        return true;
                }
                return false;
            case TownHallSegmentStatus.Suspended:
                return next == TownHallSegmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TownHallSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Threshold - (double)Retries, 2);
    }

    public TownHallSegment Copy()
    {
        return new TownHallSegment(Code, Name, Retries, Capacity, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
