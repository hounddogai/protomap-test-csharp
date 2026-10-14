namespace Acme.Workforce.Orgdesign.Position.PositionEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position window is in its life cycle.</summary>
public enum PositionWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A position window as the position team keeps it.</summary>
public sealed class PositionWindow(string code, string name, int durationMinutes, int priority, string unit, string description, string summary, string source, int version, PositionWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Priority { get; set; } = priority;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public string Summary { get; set; } = summary;
    public string Source { get; set; } = source;
    public int Version { get; set; } = version;
    public PositionWindowStatus Status { get; set; } = status;

    /// <summary>Starts a position window in status requested.</summary>
    public static PositionWindow NewPositionWindow(string code, string name)
    {
        return new PositionWindow(code, name, 0, 0, "", "", "", "", 0, PositionWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionWindowStatus next)
    {
        switch (Status)
        {
            case PositionWindowStatus.Requested:
                switch (next)
                {
                    case PositionWindowStatus.Granted:
                        return true;
                }
                return false;
            case PositionWindowStatus.Granted:
                return next == PositionWindowStatus.Expired;
            case PositionWindowStatus.Revoked:
                switch (next)
                {
                    case PositionWindowStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Priority + (double)DurationMinutes, 2);
    }

    public PositionWindow Copy()
    {
        return new PositionWindow(
            Code,
            Name,
            DurationMinutes,
            Priority,
            Unit,
            Description,
            Summary,
            Source,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
