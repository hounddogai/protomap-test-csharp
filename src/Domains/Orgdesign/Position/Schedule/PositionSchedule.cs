namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position schedule is in its life cycle.</summary>
public enum PositionScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A position schedule as the position team keeps it.</summary>
public sealed class PositionSchedule(string code, string label, int tenureMonths, string title, double weight, string department, PositionScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public string Department { get; set; } = department;
    public PositionScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a position schedule in status new.</summary>
    public static PositionSchedule NewPositionSchedule(string code, string label)
    {
        return new PositionSchedule(code, label, 0, "", 0.0, "", PositionScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionScheduleStatus next)
    {
        switch (Status)
        {
            case PositionScheduleStatus.New:
                switch (next)
                {
                    case PositionScheduleStatus.Queued:
                        return true;
                }
                return false;
            case PositionScheduleStatus.Queued:
                switch (next)
                {
                    case PositionScheduleStatus.Done:
                    case PositionScheduleStatus.Failed:
                        return true;
                }
                return false;
            case PositionScheduleStatus.Running:
                return next == PositionScheduleStatus.Done || next == PositionScheduleStatus.Failed;
            case PositionScheduleStatus.Done:
                return next == PositionScheduleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PositionScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"position schedule"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)TenureMonths * Weight, 2);
    }

    public PositionSchedule Copy()
    {
        return new PositionSchedule(Code, Label, TenureMonths, Title, Weight, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
