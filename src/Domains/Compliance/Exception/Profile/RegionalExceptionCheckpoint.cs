namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional exception checkpoint is in its life cycle.</summary>
public enum RegionalExceptionCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One regional exception checkpoint, with what the planners need to act on it.</summary>
public sealed class RegionalExceptionCheckpoint(string code, string title, string team, double ceiling, string name, RegionalExceptionCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public double Ceiling { get; set; } = ceiling;
    public string Name { get; set; } = name;
    public RegionalExceptionCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a regional exception checkpoint in status proposed.</summary>
    public static RegionalExceptionCheckpoint NewRegionalExceptionCheckpoint(string code, string title)
    {
        return new RegionalExceptionCheckpoint(code, title, "", 0.0, "", RegionalExceptionCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalExceptionCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalExceptionCheckpointStatus next)
    {
        switch (Status)
        {
            case RegionalExceptionCheckpointStatus.Proposed:
                return next == RegionalExceptionCheckpointStatus.Approved;
            case RegionalExceptionCheckpointStatus.Approved:
                switch (next)
                {
                    case RegionalExceptionCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
            case RegionalExceptionCheckpointStatus.Rejected:
                switch (next)
                {
                    case RegionalExceptionCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalExceptionCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional exception checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional exception checkpoint"} {Code}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public RegionalExceptionCheckpoint Copy()
    {
        return new RegionalExceptionCheckpoint(Code, Title, Team, Ceiling, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
