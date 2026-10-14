namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due milestone is in its life cycle.</summary>
public enum TrainingDueMilestoneStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One training due milestone, with what the planners need to act on it.</summary>
public sealed class TrainingDueMilestone(string code, string label, string ownerId, string description, string name, string jobCode, string channel, double score, TrainingDueMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Description { get; set; } = description;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Channel { get; set; } = channel;
    public double Score { get; set; } = score;
    public TrainingDueMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a training due milestone in status draft.</summary>
    public static TrainingDueMilestone NewTrainingDueMilestone(string code, string label)
    {
        return new TrainingDueMilestone(code, label, "", "", "", "", "", 0.0, TrainingDueMilestoneStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueMilestoneStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueMilestoneStatus next)
    {
        switch (Status)
        {
            case TrainingDueMilestoneStatus.Draft:
                switch (next)
                {
                    case TrainingDueMilestoneStatus.Closed:
                        return true;
                }
                return false;
            case TrainingDueMilestoneStatus.Pending:
                return next == TrainingDueMilestoneStatus.Suspended || next == TrainingDueMilestoneStatus.Closed;
            case TrainingDueMilestoneStatus.Active:
                switch (next)
                {
                    case TrainingDueMilestoneStatus.Suspended:
                        return true;
                }
                return false;
            case TrainingDueMilestoneStatus.Suspended:
                return next == TrainingDueMilestoneStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TrainingDueMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"training due milestone"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public TrainingDueMilestone Copy()
    {
        return new TrainingDueMilestone(Code, Label, OwnerId, Description, Name, JobCode, Channel, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
