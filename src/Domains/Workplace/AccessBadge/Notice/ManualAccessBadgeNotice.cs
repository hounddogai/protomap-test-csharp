namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual access badge notice is in its life cycle.</summary>
public enum ManualAccessBadgeNoticeStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One manual access badge notice, with what the planners need to act on it.</summary>
public sealed class ManualAccessBadgeNotice(string code, string name, string salaryBand, int priority, double rating, string costCenter, ManualAccessBadgeNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public int Priority { get; set; } = priority;
    public double Rating { get; set; } = rating;
    public string CostCenter { get; set; } = costCenter;
    public ManualAccessBadgeNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a manual access badge notice in status planned.</summary>
    public static ManualAccessBadgeNotice NewManualAccessBadgeNotice(string code, string name)
    {
        return new ManualAccessBadgeNotice(code, name, "", 0, 0.0, "", ManualAccessBadgeNoticeStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAccessBadgeNoticeStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAccessBadgeNoticeStatus next)
    {
        switch (Status)
        {
            case ManualAccessBadgeNoticeStatus.Planned:
                switch (next)
                {
                    case ManualAccessBadgeNoticeStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualAccessBadgeNoticeStatus.Scheduled:
                return next == ManualAccessBadgeNoticeStatus.Cancelled;
            case ManualAccessBadgeNoticeStatus.InProgress:
                return next == ManualAccessBadgeNoticeStatus.Completed || next == ManualAccessBadgeNoticeStatus.Cancelled;
            case ManualAccessBadgeNoticeStatus.Completed:
                return next == ManualAccessBadgeNoticeStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ManualAccessBadgeNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual access badge notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Priority * Rating, 2);
    }

    public ManualAccessBadgeNotice Copy()
    {
        return new ManualAccessBadgeNotice(Code, Name, SalaryBand, Priority, Rating, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
