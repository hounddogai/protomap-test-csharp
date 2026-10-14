namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request checkpoint is in its life cycle.</summary>
public enum LeaveRequestCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a leave request checkpoint and its current status.</summary>
public sealed class LeaveRequestCheckpoint(string code, string title, string grade, Dictionary<string, string> attributes, string costCenter, double weight, int priority, double maximum, LeaveRequestCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string CostCenter { get; set; } = costCenter;
    public double Weight { get; set; } = weight;
    public int Priority { get; set; } = priority;
    public double Maximum { get; set; } = maximum;
    public LeaveRequestCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a leave request checkpoint in status new.</summary>
    public static LeaveRequestCheckpoint NewLeaveRequestCheckpoint(string code, string title)
    {
        return new LeaveRequestCheckpoint(
            code,
            title,
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            0,
            0.0,
            LeaveRequestCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestCheckpointStatus next)
    {
        switch (Status)
        {
            case LeaveRequestCheckpointStatus.New:
                return next == LeaveRequestCheckpointStatus.Running;
            case LeaveRequestCheckpointStatus.Queued:
                return next == LeaveRequestCheckpointStatus.Done || next == LeaveRequestCheckpointStatus.Failed;
            case LeaveRequestCheckpointStatus.Running:
                return next == LeaveRequestCheckpointStatus.Done;
            case LeaveRequestCheckpointStatus.Done:
                return next == LeaveRequestCheckpointStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LeaveRequestCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double WeightedExposure()
    {
        return Math.Round((double)Priority - Maximum, 2);
    }

    public LeaveRequestCheckpoint Copy()
    {
        return new LeaveRequestCheckpoint(
            Code,
            Title,
            Grade,
            Attributes,
            CostCenter,
            Weight,
            Priority,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
