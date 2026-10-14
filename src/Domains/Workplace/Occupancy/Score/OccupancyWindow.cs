namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy window is in its life cycle.</summary>
public enum OccupancyWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A occupancy window as the occupancy team keeps it.</summary>
public sealed class OccupancyWindow(string code, string label, double hours, double discount, double weight, double minimum, bool active, string skill, OccupancyWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public double Discount { get; set; } = discount;
    public double Weight { get; set; } = weight;
    public double Minimum { get; set; } = minimum;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public OccupancyWindowStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy window in status new.</summary>
    public static OccupancyWindow NewOccupancyWindow(string code, string label)
    {
        return new OccupancyWindow(code, label, 0.0, 0.0, 0.0, 0.0, false, "", OccupancyWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyWindowStatus next)
    {
        switch (Status)
        {
            case OccupancyWindowStatus.New:
                return next == OccupancyWindowStatus.Queued || next == OccupancyWindowStatus.Failed;
            case OccupancyWindowStatus.Queued:
                return next == OccupancyWindowStatus.Done;
            case OccupancyWindowStatus.Running:
                switch (next)
                {
                    case OccupancyWindowStatus.Done:
                        return true;
                }
                return false;
            case OccupancyWindowStatus.Done:
                switch (next)
                {
                    case OccupancyWindowStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Minimum - Weight, 2);
    }

    public OccupancyWindow Copy()
    {
        return new OccupancyWindow(Code, Label, Hours, Discount, Weight, Minimum, Active, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
