namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal license seat assignment is in its life cycle.</summary>
public enum SeasonalLicenseSeatAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One seasonal license seat assignment, with what the planners need to act on it.</summary>
public sealed class SeasonalLicenseSeatAssignment(string code, string name, string team, double cost, int sequence, SeasonalLicenseSeatAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public SeasonalLicenseSeatAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal license seat assignment in status proposed.</summary>
    public static SeasonalLicenseSeatAssignment NewSeasonalLicenseSeatAssignment(string code, string name)
    {
        return new SeasonalLicenseSeatAssignment(
            code,
            name,
            "",
            0.0,
            0,
            SeasonalLicenseSeatAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalLicenseSeatAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalLicenseSeatAssignmentStatus next)
    {
        switch (Status)
        {
            case SeasonalLicenseSeatAssignmentStatus.Proposed:
                switch (next)
                {
                    case SeasonalLicenseSeatAssignmentStatus.Approved:
                        return true;
                }
                return false;
            case SeasonalLicenseSeatAssignmentStatus.Approved:
                switch (next)
                {
                    case SeasonalLicenseSeatAssignmentStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalLicenseSeatAssignmentStatus.Rejected:
                switch (next)
                {
                    case SeasonalLicenseSeatAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalLicenseSeatAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal license seat assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal license seat assignment"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Sequence + Cost, 2);
    }

    public SeasonalLicenseSeatAssignment Copy()
    {
        return new SeasonalLicenseSeatAssignment(Code, Name, Team, Cost, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
