namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment checkpoint is in its life cycle.</summary>
public enum AssignmentCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A assignment checkpoint. Its code is unique within a tenant.</summary>
public sealed class AssignmentCheckpoint(string code, string label, int createdAt, string skill, int version, int startsAt, double weight, int graceDays, int leaveDays, AssignmentCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public string Skill { get; set; } = skill;
    public int Version { get; set; } = version;
    public int StartsAt { get; set; } = startsAt;
    public double Weight { get; set; } = weight;
    public int GraceDays { get; set; } = graceDays;
    public int LeaveDays { get; set; } = leaveDays;
    public AssignmentCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a assignment checkpoint in status new.</summary>
    public static AssignmentCheckpoint NewAssignmentCheckpoint(string code, string label)
    {
        return new AssignmentCheckpoint(code, label, 0, "", 0, 0, 0.0, 0, 0, AssignmentCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentCheckpointStatus next)
    {
        switch (Status)
        {
            case AssignmentCheckpointStatus.New:
                return next == AssignmentCheckpointStatus.Running;
            case AssignmentCheckpointStatus.Queued:
                switch (next)
                {
                    case AssignmentCheckpointStatus.Failed:
                        return true;
                }
                return false;
            case AssignmentCheckpointStatus.Running:
                switch (next)
                {
                    case AssignmentCheckpointStatus.Done:
                    case AssignmentCheckpointStatus.Failed:
                        return true;
                }
                return false;
            case AssignmentCheckpointStatus.Done:
                return next == AssignmentCheckpointStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AssignmentCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)StartsAt - (double)Version, 2);
    }

    public AssignmentCheckpoint Copy()
    {
        return new AssignmentCheckpoint(
            Code,
            Label,
            CreatedAt,
            Skill,
            Version,
            StartsAt,
            Weight,
            GraceDays,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
