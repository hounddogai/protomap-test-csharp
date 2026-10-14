namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment batch is in its life cycle.</summary>
public enum AssignmentBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One assignment batch, with what the planners need to act on it.</summary>
public sealed class AssignmentBatch(string code, string name, double maximum, double hours, int windowDays, int capacity, string skill, double minimum, string description, double score, AssignmentBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public double Hours { get; set; } = hours;
    public int WindowDays { get; set; } = windowDays;
    public int Capacity { get; set; } = capacity;
    public string Skill { get; set; } = skill;
    public double Minimum { get; set; } = minimum;
    public string Description { get; set; } = description;
    public double Score { get; set; } = score;
    public AssignmentBatchStatus Status { get; set; } = status;

    /// <summary>Starts a assignment batch in status requested.</summary>
    public static AssignmentBatch NewAssignmentBatch(string code, string name)
    {
        return new AssignmentBatch(code, name, 0.0, 0.0, 0, 0, "", 0.0, "", 0.0, AssignmentBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentBatchStatus next)
    {
        switch (Status)
        {
            case AssignmentBatchStatus.Requested:
                return next == AssignmentBatchStatus.Revoked || next == AssignmentBatchStatus.Expired;
            case AssignmentBatchStatus.Granted:
                switch (next)
                {
                    case AssignmentBatchStatus.Revoked:
                        return true;
                }
                return false;
            case AssignmentBatchStatus.Revoked:
                return next == AssignmentBatchStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AssignmentBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"assignment batch"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)WindowDays + Score, 2);
    }

    public AssignmentBatch Copy()
    {
        return new AssignmentBatch(
            Code,
            Name,
            Maximum,
            Hours,
            WindowDays,
            Capacity,
            Skill,
            Minimum,
            Description,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
