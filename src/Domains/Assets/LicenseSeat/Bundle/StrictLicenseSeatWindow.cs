namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict license seat window is in its life cycle.</summary>
public enum StrictLicenseSeatWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A strict license seat window. Its code is unique within a tenant.</summary>
public sealed class StrictLicenseSeatWindow(string code, string label, int limit, string unit, double cost, string employeeId, StrictLicenseSeatWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string Unit { get; set; } = unit;
    public double Cost { get; set; } = cost;
    public string EmployeeId { get; set; } = employeeId;
    public StrictLicenseSeatWindowStatus Status { get; set; } = status;

    /// <summary>Starts a strict license seat window in status requested.</summary>
    public static StrictLicenseSeatWindow NewStrictLicenseSeatWindow(string code, string label)
    {
        return new StrictLicenseSeatWindow(code, label, 0, "", 0.0, "", StrictLicenseSeatWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictLicenseSeatWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictLicenseSeatWindowStatus next)
    {
        switch (Status)
        {
            case StrictLicenseSeatWindowStatus.Requested:
                switch (next)
                {
                    case StrictLicenseSeatWindowStatus.Revoked:
                        return true;
                }
                return false;
            case StrictLicenseSeatWindowStatus.Granted:
                return next == StrictLicenseSeatWindowStatus.Revoked || next == StrictLicenseSeatWindowStatus.Expired;
            case StrictLicenseSeatWindowStatus.Revoked:
                switch (next)
                {
                    case StrictLicenseSeatWindowStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictLicenseSeatWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict license seat window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Cost + (double)Limit, 2);
    }

    public StrictLicenseSeatWindow Copy()
    {
        return new StrictLicenseSeatWindow(Code, Label, Limit, Unit, Cost, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
