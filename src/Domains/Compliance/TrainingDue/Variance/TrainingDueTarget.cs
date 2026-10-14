namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due target is in its life cycle.</summary>
public enum TrainingDueTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a training due target and its current status.</summary>
public sealed class TrainingDueTarget(string code, string name, double floorValue, string location, string contactEmail, string managerId, string grade, Dictionary<string, string> attributes, bool approved, string skill, TrainingDueTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Location { get; set; } = location;
    public string ContactEmail { get; set; } = contactEmail;
    public string ManagerId { get; set; } = managerId;
    public string Grade { get; set; } = grade;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Approved { get; set; } = approved;
    public string Skill { get; set; } = skill;
    public TrainingDueTargetStatus Status { get; set; } = status;

    /// <summary>Starts a training due target in status idle.</summary>
    public static TrainingDueTarget NewTrainingDueTarget(string code, string name)
    {
        return new TrainingDueTarget(
            code,
            name,
            0.0,
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            false,
            "",
            TrainingDueTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueTargetStatus next)
    {
        switch (Status)
        {
            case TrainingDueTargetStatus.Idle:
                return next == TrainingDueTargetStatus.Stopped;
            case TrainingDueTargetStatus.Warming:
                return next == TrainingDueTargetStatus.Ready || next == TrainingDueTargetStatus.Stopped;
            case TrainingDueTargetStatus.Ready:
                switch (next)
                {
                    case TrainingDueTargetStatus.Draining:
                    case TrainingDueTargetStatus.Stopped:
                        return true;
                }
                return false;
            case TrainingDueTargetStatus.Draining:
                return next == TrainingDueTargetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TrainingDueTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public TrainingDueTarget Copy()
    {
        return new TrainingDueTarget(
            Code,
            Name,
            FloorValue,
            Location,
            ContactEmail,
            ManagerId,
            Grade,
            Attributes,
            Approved,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
