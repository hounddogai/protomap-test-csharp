namespace Acme.Workforce.Orgdesign.Position.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position plan is in its life cycle.</summary>
public enum PositionPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a position plan and its current status.</summary>
public sealed class PositionPlan(string code, string name, string channel, int durationMinutes, double cost, string notes, string team, PositionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Cost { get; set; } = cost;
    public string Notes { get; set; } = notes;
    public string Team { get; set; } = team;
    public PositionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a position plan in status new.</summary>
    public static PositionPlan NewPositionPlan(string code, string name)
    {
        return new PositionPlan(code, name, "", 0, 0.0, "", "", PositionPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionPlanStatus next)
    {
        switch (Status)
        {
            case PositionPlanStatus.New:
                switch (next)
                {
                    case PositionPlanStatus.Queued:
                    case PositionPlanStatus.Failed:
                        return true;
                }
                return false;
            case PositionPlanStatus.Queued:
                switch (next)
                {
                    case PositionPlanStatus.Failed:
                        return true;
                }
                return false;
            case PositionPlanStatus.Running:
                return next == PositionPlanStatus.Done || next == PositionPlanStatus.Failed;
            case PositionPlanStatus.Done:
                switch (next)
                {
                    case PositionPlanStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"position plan"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)DurationMinutes + Cost, 2);
    }

    public PositionPlan Copy()
    {
        return new PositionPlan(Code, Name, Channel, DurationMinutes, Cost, Notes, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
