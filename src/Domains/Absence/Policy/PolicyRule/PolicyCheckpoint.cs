namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy checkpoint is in its life cycle.</summary>
public enum PolicyCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One policy checkpoint, with what the planners need to act on it.</summary>
public sealed class PolicyCheckpoint(string code, string label, int windowDays, int updatedAt, string title, string grade, double hours, bool visible, string reviewer, string department, PolicyCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public double Hours { get; set; } = hours;
    public bool Visible { get; set; } = visible;
    public string Reviewer { get; set; } = reviewer;
    public string Department { get; set; } = department;
    public PolicyCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a policy checkpoint in status idle.</summary>
    public static PolicyCheckpoint NewPolicyCheckpoint(string code, string label)
    {
        return new PolicyCheckpoint(code, label, 0, 0, "", "", 0.0, false, "", "", PolicyCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyCheckpointStatus next)
    {
        switch (Status)
        {
            case PolicyCheckpointStatus.Idle:
                return next == PolicyCheckpointStatus.Ready;
            case PolicyCheckpointStatus.Warming:
                return next == PolicyCheckpointStatus.Draining;
            case PolicyCheckpointStatus.Ready:
                switch (next)
                {
                    case PolicyCheckpointStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyCheckpointStatus.Draining:
                switch (next)
                {
                    case PolicyCheckpointStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Hours + (double)UpdatedAt, 2);
    }

    public PolicyCheckpoint Copy()
    {
        return new PolicyCheckpoint(
            Code,
            Label,
            WindowDays,
            UpdatedAt,
            Title,
            Grade,
            Hours,
            Visible,
            Reviewer,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
