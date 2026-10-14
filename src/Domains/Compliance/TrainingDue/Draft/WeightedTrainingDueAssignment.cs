namespace Acme.Workforce.Compliance.TrainingDue.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted training due assignment is in its life cycle.</summary>
public enum WeightedTrainingDueAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A weighted training due assignment. Its code is unique within a tenant.</summary>
public sealed class WeightedTrainingDueAssignment(string code, string label, string reviewer, string team, string segment, int startsAt, WeightedTrainingDueAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Team { get; set; } = team;
    public string Segment { get; set; } = segment;
    public int StartsAt { get; set; } = startsAt;
    public WeightedTrainingDueAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a weighted training due assignment in status new.</summary>
    public static WeightedTrainingDueAssignment NewWeightedTrainingDueAssignment(string code, string label)
    {
        return new WeightedTrainingDueAssignment(
            code,
            label,
            "",
            "",
            "",
            0,
            WeightedTrainingDueAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedTrainingDueAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedTrainingDueAssignmentStatus next)
    {
        switch (Status)
        {
            case WeightedTrainingDueAssignmentStatus.New:
                return next == WeightedTrainingDueAssignmentStatus.Queued || next == WeightedTrainingDueAssignmentStatus.Failed;
            case WeightedTrainingDueAssignmentStatus.Queued:
                return next == WeightedTrainingDueAssignmentStatus.Running || next == WeightedTrainingDueAssignmentStatus.Failed;
            case WeightedTrainingDueAssignmentStatus.Running:
                return next == WeightedTrainingDueAssignmentStatus.Done || next == WeightedTrainingDueAssignmentStatus.Failed;
            case WeightedTrainingDueAssignmentStatus.Done:
                switch (next)
                {
                    case WeightedTrainingDueAssignmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedTrainingDueAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted training due assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public WeightedTrainingDueAssignment Copy()
    {
        return new WeightedTrainingDueAssignment(Code, Label, Reviewer, Team, Segment, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
