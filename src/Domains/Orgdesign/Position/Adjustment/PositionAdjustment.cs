namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position adjustment is in its life cycle.</summary>
public enum PositionAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One position adjustment, with what the planners need to act on it.</summary>
public sealed class PositionAdjustment(string code, string name, string jobCode, double rating, bool archived, int createdAt, double hours, PositionAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public double Rating { get; set; } = rating;
    public bool Archived { get; set; } = archived;
    public int CreatedAt { get; set; } = createdAt;
    public double Hours { get; set; } = hours;
    public PositionAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a position adjustment in status new.</summary>
    public static PositionAdjustment NewPositionAdjustment(string code, string name)
    {
        return new PositionAdjustment(code, name, "", 0.0, false, 0, 0.0, PositionAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionAdjustmentStatus next)
    {
        switch (Status)
        {
            case PositionAdjustmentStatus.New:
                switch (next)
                {
                    case PositionAdjustmentStatus.Queued:
                        return true;
                }
                return false;
            case PositionAdjustmentStatus.Queued:
                switch (next)
                {
                    case PositionAdjustmentStatus.Running:
                        return true;
                }
                return false;
            case PositionAdjustmentStatus.Running:
                switch (next)
                {
                    case PositionAdjustmentStatus.Done:
                    case PositionAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case PositionAdjustmentStatus.Done:
                return next == PositionAdjustmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PositionAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Hours * Rating, 2);
    }

    public PositionAdjustment Copy()
    {
        return new PositionAdjustment(Code, Name, JobCode, Rating, Archived, CreatedAt, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
