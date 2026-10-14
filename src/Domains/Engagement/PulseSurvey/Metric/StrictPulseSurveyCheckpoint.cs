namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pulse survey checkpoint is in its life cycle.</summary>
public enum StrictPulseSurveyCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A strict pulse survey checkpoint. Its code is unique within a tenant.</summary>
public sealed class StrictPulseSurveyCheckpoint(string code, string label, bool pinned, int startsAt, string description, bool active, string department, StrictPulseSurveyCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public int StartsAt { get; set; } = startsAt;
    public string Description { get; set; } = description;
    public bool Active { get; set; } = active;
    public string Department { get; set; } = department;
    public StrictPulseSurveyCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a strict pulse survey checkpoint in status idle.</summary>
    public static StrictPulseSurveyCheckpoint NewStrictPulseSurveyCheckpoint(string code, string label)
    {
        return new StrictPulseSurveyCheckpoint(
            code,
            label,
            false,
            0,
            "",
            false,
            "",
            StrictPulseSurveyCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPulseSurveyCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPulseSurveyCheckpointStatus next)
    {
        switch (Status)
        {
            case StrictPulseSurveyCheckpointStatus.Idle:
                return next == StrictPulseSurveyCheckpointStatus.Draining || next == StrictPulseSurveyCheckpointStatus.Stopped;
            case StrictPulseSurveyCheckpointStatus.Warming:
                return next == StrictPulseSurveyCheckpointStatus.Ready || next == StrictPulseSurveyCheckpointStatus.Stopped;
            case StrictPulseSurveyCheckpointStatus.Ready:
                switch (next)
                {
                    case StrictPulseSurveyCheckpointStatus.Draining:
                    case StrictPulseSurveyCheckpointStatus.Stopped:
                        return true;
                }
                return false;
            case StrictPulseSurveyCheckpointStatus.Draining:
                return next == StrictPulseSurveyCheckpointStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictPulseSurveyCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pulse survey checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public StrictPulseSurveyCheckpoint Copy()
    {
        return new StrictPulseSurveyCheckpoint(
            Code,
            Label,
            Pinned,
            StartsAt,
            Description,
            Active,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
