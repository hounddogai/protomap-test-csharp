namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk quota is in its life cycle.</summary>
public enum DeskQuotaStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A desk quota. Its code is unique within a tenant.</summary>
public sealed class DeskQuota(string code, string title, int leaveDays, string employeeId, int retries, string phone, bool approved, double budget, int durationMinutes, string region, string description, DeskQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public string Phone { get; set; } = phone;
    public bool Approved { get; set; } = approved;
    public double Budget { get; set; } = budget;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Region { get; set; } = region;
    public string Description { get; set; } = description;
    public DeskQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a desk quota in status new.</summary>
    public static DeskQuota NewDeskQuota(string code, string title)
    {
        return new DeskQuota(code, title, 0, "", 0, "", false, 0.0, 0, "", "", DeskQuotaStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskQuotaStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskQuotaStatus next)
    {
        switch (Status)
        {
            case DeskQuotaStatus.New:
                return next == DeskQuotaStatus.Queued || next == DeskQuotaStatus.Failed;
            case DeskQuotaStatus.Queued:
                return next == DeskQuotaStatus.Done || next == DeskQuotaStatus.Failed;
            case DeskQuotaStatus.Running:
                return next == DeskQuotaStatus.Done;
            case DeskQuotaStatus.Done:
                switch (next)
                {
                    case DeskQuotaStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Retries + (double)DurationMinutes, 2);
    }

    public DeskQuota Copy()
    {
        return new DeskQuota(
            Code,
            Title,
            LeaveDays,
            EmployeeId,
            Retries,
            Phone,
            Approved,
            Budget,
            DurationMinutes,
            Region,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
