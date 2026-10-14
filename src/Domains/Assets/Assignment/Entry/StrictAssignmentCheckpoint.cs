namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict assignment checkpoint is in its life cycle.</summary>
public enum StrictAssignmentCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One strict assignment checkpoint, with what the planners need to act on it.</summary>
public sealed class StrictAssignmentCheckpoint(string code, string label, double rate, string notes, int graceDays, string currency, double hours, int createdAt, StrictAssignmentCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Notes { get; set; } = notes;
    public int GraceDays { get; set; } = graceDays;
    public string Currency { get; set; } = currency;
    public double Hours { get; set; } = hours;
    public int CreatedAt { get; set; } = createdAt;
    public StrictAssignmentCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a strict assignment checkpoint in status created.</summary>
    public static StrictAssignmentCheckpoint NewStrictAssignmentCheckpoint(string code, string label)
    {
        return new StrictAssignmentCheckpoint(
            code,
            label,
            0.0,
            "",
            0,
            "",
            0.0,
            0,
            StrictAssignmentCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAssignmentCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAssignmentCheckpointStatus next)
    {
        switch (Status)
        {
            case StrictAssignmentCheckpointStatus.Created:
                return next == StrictAssignmentCheckpointStatus.Validated || next == StrictAssignmentCheckpointStatus.Retired;
            case StrictAssignmentCheckpointStatus.Validated:
                switch (next)
                {
                    case StrictAssignmentCheckpointStatus.Retired:
                        return true;
                }
                return false;
            case StrictAssignmentCheckpointStatus.Published:
                return next == StrictAssignmentCheckpointStatus.Retired;
        }
        return false;
    }

    public void MoveTo(StrictAssignmentCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict assignment checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Rate - (double)CreatedAt, 2);
    }

    public StrictAssignmentCheckpoint Copy()
    {
        return new StrictAssignmentCheckpoint(
            Code,
            Label,
            Rate,
            Notes,
            GraceDays,
            Currency,
            Hours,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
