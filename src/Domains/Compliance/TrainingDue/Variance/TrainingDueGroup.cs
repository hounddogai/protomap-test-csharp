namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due group is in its life cycle.</summary>
public enum TrainingDueGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One training due group, with what the planners need to act on it.</summary>
public sealed class TrainingDueGroup(string code, string title, int windowDays, string location, int priority, string currency, string label, TrainingDueGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public string Location { get; set; } = location;
    public int Priority { get; set; } = priority;
    public string Currency { get; set; } = currency;
    public string Label { get; set; } = label;
    public TrainingDueGroupStatus Status { get; set; } = status;

    /// <summary>Starts a training due group in status idle.</summary>
    public static TrainingDueGroup NewTrainingDueGroup(string code, string title)
    {
        return new TrainingDueGroup(code, title, 0, "", 0, "", "", TrainingDueGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueGroupStatus next)
    {
        switch (Status)
        {
            case TrainingDueGroupStatus.Idle:
                return next == TrainingDueGroupStatus.Warming || next == TrainingDueGroupStatus.Draining;
            case TrainingDueGroupStatus.Warming:
                return next == TrainingDueGroupStatus.Draining;
            case TrainingDueGroupStatus.Ready:
                return next == TrainingDueGroupStatus.Draining;
            case TrainingDueGroupStatus.Draining:
                switch (next)
                {
                    case TrainingDueGroupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)WindowDays - (double)Priority, 2);
    }

    public TrainingDueGroup Copy()
    {
        return new TrainingDueGroup(Code, Title, WindowDays, Location, Priority, Currency, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
