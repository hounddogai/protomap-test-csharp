namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal garnishment notice is in its life cycle.</summary>
public enum SeasonalGarnishmentNoticeStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal garnishment notice. Its code is unique within a tenant.</summary>
public sealed class SeasonalGarnishmentNotice(string code, string label, int tenureMonths, string employeeId, double margin, string jobCode, int retries, string category, SeasonalGarnishmentNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string EmployeeId { get; set; } = employeeId;
    public double Margin { get; set; } = margin;
    public string JobCode { get; set; } = jobCode;
    public int Retries { get; set; } = retries;
    public string Category { get; set; } = category;
    public SeasonalGarnishmentNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal garnishment notice in status planned.</summary>
    public static SeasonalGarnishmentNotice NewSeasonalGarnishmentNotice(string code, string label)
    {
        return new SeasonalGarnishmentNotice(
            code,
            label,
            0,
            "",
            0.0,
            "",
            0,
            "",
            SeasonalGarnishmentNoticeStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalGarnishmentNoticeStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalGarnishmentNoticeStatus next)
    {
        switch (Status)
        {
            case SeasonalGarnishmentNoticeStatus.Planned:
                return next == SeasonalGarnishmentNoticeStatus.Scheduled;
            case SeasonalGarnishmentNoticeStatus.Scheduled:
                switch (next)
                {
                    case SeasonalGarnishmentNoticeStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalGarnishmentNoticeStatus.InProgress:
                return next == SeasonalGarnishmentNoticeStatus.Completed;
            case SeasonalGarnishmentNoticeStatus.Completed:
                switch (next)
                {
                    case SeasonalGarnishmentNoticeStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalGarnishmentNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal garnishment notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"seasonal garnishment notice"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)TenureMonths * Margin, 2);
    }

    public SeasonalGarnishmentNotice Copy()
    {
        return new SeasonalGarnishmentNotice(
            Code,
            Label,
            TenureMonths,
            EmployeeId,
            Margin,
            JobCode,
            Retries,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
