namespace Acme.Workforce.Compliance.TrainingDue.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due window is in its life cycle.</summary>
public enum TrainingDueWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One training due window, with what the planners need to act on it.</summary>
public sealed class TrainingDueWindow(string code, string title, string channel, double rating, bool active, int version, bool visible, string salaryBand, double weight, double ceiling, TrainingDueWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public double Rating { get; set; } = rating;
    public bool Active { get; set; } = active;
    public int Version { get; set; } = version;
    public bool Visible { get; set; } = visible;
    public string SalaryBand { get; set; } = salaryBand;
    public double Weight { get; set; } = weight;
    public double Ceiling { get; set; } = ceiling;
    public TrainingDueWindowStatus Status { get; set; } = status;

    /// <summary>Starts a training due window in status planned.</summary>
    public static TrainingDueWindow NewTrainingDueWindow(string code, string title)
    {
        return new TrainingDueWindow(
            code,
            title,
            "",
            0.0,
            false,
            0,
            false,
            "",
            0.0,
            0.0,
            TrainingDueWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueWindowStatus next)
    {
        switch (Status)
        {
            case TrainingDueWindowStatus.Planned:
                return next == TrainingDueWindowStatus.InProgress || next == TrainingDueWindowStatus.Cancelled;
            case TrainingDueWindowStatus.Scheduled:
                return next == TrainingDueWindowStatus.Completed || next == TrainingDueWindowStatus.Cancelled;
            case TrainingDueWindowStatus.InProgress:
                switch (next)
                {
                    case TrainingDueWindowStatus.Completed:
                    case TrainingDueWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case TrainingDueWindowStatus.Completed:
                switch (next)
                {
                    case TrainingDueWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Rating - Ceiling, 2);
    }

    public TrainingDueWindow Copy()
    {
        return new TrainingDueWindow(
            Code,
            Title,
            Channel,
            Rating,
            Active,
            Version,
            Visible,
            SalaryBand,
            Weight,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
