namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall record is in its life cycle.</summary>
public enum TownHallRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a town hall record and its current status.</summary>
public sealed class TownHallRecord(string code, string title, string skill, double floorValue, string badge, double margin, int limit, int createdAt, int windowDays, TownHallRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public double FloorValue { get; set; } = floorValue;
    public string Badge { get; set; } = badge;
    public double Margin { get; set; } = margin;
    public int Limit { get; set; } = limit;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public TownHallRecordStatus Status { get; set; } = status;

    /// <summary>Starts a town hall record in status idle.</summary>
    public static TownHallRecord NewTownHallRecord(string code, string title)
    {
        return new TownHallRecord(code, title, "", 0.0, "", 0.0, 0, 0, 0, TownHallRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallRecordStatus next)
    {
        switch (Status)
        {
            case TownHallRecordStatus.Idle:
                return next == TownHallRecordStatus.Warming || next == TownHallRecordStatus.Stopped;
            case TownHallRecordStatus.Warming:
                return next == TownHallRecordStatus.Draining;
            case TownHallRecordStatus.Ready:
                switch (next)
                {
                    case TownHallRecordStatus.Draining:
                        return true;
                }
                return false;
            case TownHallRecordStatus.Draining:
                switch (next)
                {
                    case TownHallRecordStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"town hall record"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Margin * (double)CreatedAt, 2);
    }

    public TownHallRecord Copy()
    {
        return new TownHallRecord(
            Code,
            Title,
            Skill,
            FloorValue,
            Badge,
            Margin,
            Limit,
            CreatedAt,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
