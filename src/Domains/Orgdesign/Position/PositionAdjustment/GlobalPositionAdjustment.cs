namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global position adjustment is in its life cycle.</summary>
public enum GlobalPositionAdjustmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A global position adjustment. Its code is unique within a tenant.</summary>
public sealed class GlobalPositionAdjustment(string code, string name, double amount, bool archived, double score, string segment, int priority, double rating, GlobalPositionAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public bool Archived { get; set; } = archived;
    public double Score { get; set; } = score;
    public string Segment { get; set; } = segment;
    public int Priority { get; set; } = priority;
    public double Rating { get; set; } = rating;
    public GlobalPositionAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a global position adjustment in status proposed.</summary>
    public static GlobalPositionAdjustment NewGlobalPositionAdjustment(string code, string name)
    {
        return new GlobalPositionAdjustment(
            code,
            name,
            0.0,
            false,
            0.0,
            "",
            0,
            0.0,
            GlobalPositionAdjustmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPositionAdjustmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPositionAdjustmentStatus next)
    {
        switch (Status)
        {
            case GlobalPositionAdjustmentStatus.Proposed:
                switch (next)
                {
                    case GlobalPositionAdjustmentStatus.Approved:
                    case GlobalPositionAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case GlobalPositionAdjustmentStatus.Approved:
                switch (next)
                {
                    case GlobalPositionAdjustmentStatus.Rejected:
                    case GlobalPositionAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case GlobalPositionAdjustmentStatus.Rejected:
                return next == GlobalPositionAdjustmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(GlobalPositionAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global position adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global position adjustment"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Amount + Rating, 2);
    }

    public GlobalPositionAdjustment Copy()
    {
        return new GlobalPositionAdjustment(Code, Name, Amount, Archived, Score, Segment, Priority, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
