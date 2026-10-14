namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending accrual window is in its life cycle.</summary>
public enum PendingAccrualWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pending accrual window and its current status.</summary>
public sealed class PendingAccrualWindow(string code, string title, int startsAt, string salaryBand, string location, string category, PendingAccrualWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public string Location { get; set; } = location;
    public string Category { get; set; } = category;
    public PendingAccrualWindowStatus Status { get; set; } = status;

    /// <summary>Starts a pending accrual window in status proposed.</summary>
    public static PendingAccrualWindow NewPendingAccrualWindow(string code, string title)
    {
        return new PendingAccrualWindow(code, title, 0, "", "", "", PendingAccrualWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAccrualWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAccrualWindowStatus next)
    {
        switch (Status)
        {
            case PendingAccrualWindowStatus.Proposed:
                return next == PendingAccrualWindowStatus.Withdrawn;
            case PendingAccrualWindowStatus.Approved:
                switch (next)
                {
                    case PendingAccrualWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case PendingAccrualWindowStatus.Rejected:
                switch (next)
                {
                    case PendingAccrualWindowStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingAccrualWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending accrual window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public PendingAccrualWindow Copy()
    {
        return new PendingAccrualWindow(Code, Title, StartsAt, SalaryBand, Location, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
