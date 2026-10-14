namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched timesheet summary is in its life cycle.</summary>
public enum BatchedTimesheetSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A batched timesheet summary as the timesheet team keeps it.</summary>
public sealed class BatchedTimesheetSummary(string code, string title, string locale, string region, double minimum, bool active, string salaryBand, int tenureMonths, string jobCode, int durationMinutes, string employeeId, BatchedTimesheetSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string Region { get; set; } = region;
    public double Minimum { get; set; } = minimum;
    public bool Active { get; set; } = active;
    public string SalaryBand { get; set; } = salaryBand;
    public int TenureMonths { get; set; } = tenureMonths;
    public string JobCode { get; set; } = jobCode;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string EmployeeId { get; set; } = employeeId;
    public BatchedTimesheetSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a batched timesheet summary in status draft.</summary>
    public static BatchedTimesheetSummary NewBatchedTimesheetSummary(string code, string title)
    {
        return new BatchedTimesheetSummary(
            code,
            title,
            "",
            "",
            0.0,
            false,
            "",
            0,
            "",
            0,
            "",
            BatchedTimesheetSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTimesheetSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTimesheetSummaryStatus next)
    {
        switch (Status)
        {
            case BatchedTimesheetSummaryStatus.Draft:
                switch (next)
                {
                    case BatchedTimesheetSummaryStatus.Closed:
                        return true;
                }
                return false;
            case BatchedTimesheetSummaryStatus.Pending:
                return next == BatchedTimesheetSummaryStatus.Suspended || next == BatchedTimesheetSummaryStatus.Closed;
            case BatchedTimesheetSummaryStatus.Active:
                switch (next)
                {
                    case BatchedTimesheetSummaryStatus.Suspended:
                    case BatchedTimesheetSummaryStatus.Closed:
                        return true;
                }
                return false;
            case BatchedTimesheetSummaryStatus.Suspended:
                return next == BatchedTimesheetSummaryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BatchedTimesheetSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched timesheet summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched timesheet summary"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)DurationMinutes - Minimum, 2);
    }

    public BatchedTimesheetSummary Copy()
    {
        return new BatchedTimesheetSummary(
            Code,
            Title,
            Locale,
            Region,
            Minimum,
            Active,
            SalaryBand,
            TenureMonths,
            JobCode,
            DurationMinutes,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
