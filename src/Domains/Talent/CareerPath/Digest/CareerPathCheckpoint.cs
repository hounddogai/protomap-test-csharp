namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path checkpoint is in its life cycle.</summary>
public enum CareerPathCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One career path checkpoint, with what the planners need to act on it.</summary>
public sealed class CareerPathCheckpoint(string code, string label, string grade, int capacity, double ceiling, double ratio, string ownerId, double budget, string title, double weight, double fte, CareerPathCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public int Capacity { get; set; } = capacity;
    public double Ceiling { get; set; } = ceiling;
    public double Ratio { get; set; } = ratio;
    public string OwnerId { get; set; } = ownerId;
    public double Budget { get; set; } = budget;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public double Fte { get; set; } = fte;
    public CareerPathCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a career path checkpoint in status planned.</summary>
    public static CareerPathCheckpoint NewCareerPathCheckpoint(string code, string label)
    {
        return new CareerPathCheckpoint(
            code,
            label,
            "",
            0,
            0.0,
            0.0,
            "",
            0.0,
            "",
            0.0,
            0.0,
            CareerPathCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathCheckpointStatus next)
    {
        switch (Status)
        {
            case CareerPathCheckpointStatus.Planned:
                switch (next)
                {
                    case CareerPathCheckpointStatus.InProgress:
                    case CareerPathCheckpointStatus.Completed:
                        return true;
                }
                return false;
            case CareerPathCheckpointStatus.Scheduled:
                return next == CareerPathCheckpointStatus.Completed;
            case CareerPathCheckpointStatus.InProgress:
                return next == CareerPathCheckpointStatus.Completed || next == CareerPathCheckpointStatus.Cancelled;
            case CareerPathCheckpointStatus.Completed:
                switch (next)
                {
                    case CareerPathCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round(Fte + Ceiling, 2);
    }

    public CareerPathCheckpoint Copy()
    {
        return new CareerPathCheckpoint(
            Code,
            Label,
            Grade,
            Capacity,
            Ceiling,
            Ratio,
            OwnerId,
            Budget,
            Title,
            Weight,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
