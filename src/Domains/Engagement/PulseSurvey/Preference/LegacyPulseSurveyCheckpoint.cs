namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pulse survey checkpoint is in its life cycle.</summary>
public enum LegacyPulseSurveyCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One legacy pulse survey checkpoint, with what the planners need to act on it.</summary>
public sealed class LegacyPulseSurveyCheckpoint(string code, string label, int version, double minimum, string category, string managerId, int expiresAt, int quantity, double ceiling, bool visible, LegacyPulseSurveyCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public string Category { get; set; } = category;
    public string ManagerId { get; set; } = managerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Quantity { get; set; } = quantity;
    public double Ceiling { get; set; } = ceiling;
    public bool Visible { get; set; } = visible;
    public LegacyPulseSurveyCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pulse survey checkpoint in status proposed.</summary>
    public static LegacyPulseSurveyCheckpoint NewLegacyPulseSurveyCheckpoint(string code, string label)
    {
        return new LegacyPulseSurveyCheckpoint(
            code,
            label,
            0,
            0.0,
            "",
            "",
            0,
            0,
            0.0,
            false,
            LegacyPulseSurveyCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPulseSurveyCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPulseSurveyCheckpointStatus next)
    {
        switch (Status)
        {
            case LegacyPulseSurveyCheckpointStatus.Proposed:
                return next == LegacyPulseSurveyCheckpointStatus.Rejected;
            case LegacyPulseSurveyCheckpointStatus.Approved:
                return next == LegacyPulseSurveyCheckpointStatus.Rejected || next == LegacyPulseSurveyCheckpointStatus.Withdrawn;
            case LegacyPulseSurveyCheckpointStatus.Rejected:
                switch (next)
                {
                    case LegacyPulseSurveyCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPulseSurveyCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pulse survey checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round((double)ExpiresAt - (double)Version, 2);
    }

    public LegacyPulseSurveyCheckpoint Copy()
    {
        return new LegacyPulseSurveyCheckpoint(
            Code,
            Label,
            Version,
            Minimum,
            Category,
            ManagerId,
            ExpiresAt,
            Quantity,
            Ceiling,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
