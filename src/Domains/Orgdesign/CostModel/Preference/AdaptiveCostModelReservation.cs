namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive cost model reservation is in its life cycle.</summary>
public enum AdaptiveCostModelReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive cost model reservation and its current status.</summary>
public sealed class AdaptiveCostModelReservation(string code, string name, string location, string summary, double amount, string team, double budget, double hours, AdaptiveCostModelReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string Summary { get; set; } = summary;
    public double Amount { get; set; } = amount;
    public string Team { get; set; } = team;
    public double Budget { get; set; } = budget;
    public double Hours { get; set; } = hours;
    public AdaptiveCostModelReservationStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive cost model reservation in status requested.</summary>
    public static AdaptiveCostModelReservation NewAdaptiveCostModelReservation(string code, string name)
    {
        return new AdaptiveCostModelReservation(
            code,
            name,
            "",
            "",
            0.0,
            "",
            0.0,
            0.0,
            AdaptiveCostModelReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCostModelReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCostModelReservationStatus next)
    {
        switch (Status)
        {
            case AdaptiveCostModelReservationStatus.Requested:
                switch (next)
                {
                    case AdaptiveCostModelReservationStatus.Revoked:
                        return true;
                }
                return false;
            case AdaptiveCostModelReservationStatus.Granted:
                return next == AdaptiveCostModelReservationStatus.Revoked || next == AdaptiveCostModelReservationStatus.Expired;
            case AdaptiveCostModelReservationStatus.Revoked:
                switch (next)
                {
                    case AdaptiveCostModelReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveCostModelReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive cost model reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Amount + Hours, 2);
    }

    public AdaptiveCostModelReservation Copy()
    {
        return new AdaptiveCostModelReservation(Code, Name, Location, Summary, Amount, Team, Budget, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
