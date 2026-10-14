namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance window is in its life cycle.</summary>
public enum BalanceWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A balance window. Its code is unique within a tenant.</summary>
public sealed class BalanceWindow(string code, string name, string checksum, double floorValue, int graceDays, int capacity, int leaveDays, double hours, string managerId, string contactEmail, BalanceWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public double FloorValue { get; set; } = floorValue;
    public int GraceDays { get; set; } = graceDays;
    public int Capacity { get; set; } = capacity;
    public int LeaveDays { get; set; } = leaveDays;
    public double Hours { get; set; } = hours;
    public string ManagerId { get; set; } = managerId;
    public string ContactEmail { get; set; } = contactEmail;
    public BalanceWindowStatus Status { get; set; } = status;

    /// <summary>Starts a balance window in status proposed.</summary>
    public static BalanceWindow NewBalanceWindow(string code, string name)
    {
        return new BalanceWindow(code, name, "", 0.0, 0, 0, 0, 0.0, "", "", BalanceWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceWindowStatus next)
    {
        switch (Status)
        {
            case BalanceWindowStatus.Proposed:
                return next == BalanceWindowStatus.Approved || next == BalanceWindowStatus.Withdrawn;
            case BalanceWindowStatus.Approved:
                switch (next)
                {
                    case BalanceWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case BalanceWindowStatus.Rejected:
                return next == BalanceWindowStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BalanceWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)GraceDays + Hours, 2);
    }

    public BalanceWindow Copy()
    {
        return new BalanceWindow(
            Code,
            Name,
            Checksum,
            FloorValue,
            GraceDays,
            Capacity,
            LeaveDays,
            Hours,
            ManagerId,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
