namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request window is in its life cycle.</summary>
public enum LeaveRequestWindowStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A leave request window as the leave request team keeps it.</summary>
public sealed class LeaveRequestWindow(string code, string name, double rate, double discount, int createdAt, string department, int revision, bool archived, LeaveRequestWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public double Discount { get; set; } = discount;
    public int CreatedAt { get; set; } = createdAt;
    public string Department { get; set; } = department;
    public int Revision { get; set; } = revision;
    public bool Archived { get; set; } = archived;
    public LeaveRequestWindowStatus Status { get; set; } = status;

    /// <summary>Starts a leave request window in status created.</summary>
    public static LeaveRequestWindow NewLeaveRequestWindow(string code, string name)
    {
        return new LeaveRequestWindow(code, name, 0.0, 0.0, 0, "", 0, false, LeaveRequestWindowStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestWindowStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestWindowStatus next)
    {
        switch (Status)
        {
            case LeaveRequestWindowStatus.Created:
                switch (next)
                {
                    case LeaveRequestWindowStatus.Published:
                    case LeaveRequestWindowStatus.Retired:
                        return true;
                }
                return false;
            case LeaveRequestWindowStatus.Validated:
                return next == LeaveRequestWindowStatus.Retired;
            case LeaveRequestWindowStatus.Published:
                switch (next)
                {
                    case LeaveRequestWindowStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)CreatedAt - Rate, 2);
    }

    public LeaveRequestWindow Copy()
    {
        return new LeaveRequestWindow(Code, Name, Rate, Discount, CreatedAt, Department, Revision, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
