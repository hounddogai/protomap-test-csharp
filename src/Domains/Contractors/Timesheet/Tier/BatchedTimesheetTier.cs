namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched timesheet tier is in its life cycle.</summary>
public enum BatchedTimesheetTierStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One batched timesheet tier, with what the planners need to act on it.</summary>
public sealed class BatchedTimesheetTier(string code, string label, double minimum, bool approved, double maximum, double score, string currency, bool visible, string reviewer, string locale, int quantity, BatchedTimesheetTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public bool Approved { get; set; } = approved;
    public double Maximum { get; set; } = maximum;
    public double Score { get; set; } = score;
    public string Currency { get; set; } = currency;
    public bool Visible { get; set; } = visible;
    public string Reviewer { get; set; } = reviewer;
    public string Locale { get; set; } = locale;
    public int Quantity { get; set; } = quantity;
    public BatchedTimesheetTierStatus Status { get; set; } = status;

    /// <summary>Starts a batched timesheet tier in status planned.</summary>
    public static BatchedTimesheetTier NewBatchedTimesheetTier(string code, string label)
    {
        return new BatchedTimesheetTier(
            code,
            label,
            0.0,
            false,
            0.0,
            0.0,
            "",
            false,
            "",
            "",
            0,
            BatchedTimesheetTierStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTimesheetTierStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTimesheetTierStatus next)
    {
        switch (Status)
        {
            case BatchedTimesheetTierStatus.Planned:
                return next == BatchedTimesheetTierStatus.Scheduled || next == BatchedTimesheetTierStatus.Cancelled;
            case BatchedTimesheetTierStatus.Scheduled:
                return next == BatchedTimesheetTierStatus.InProgress;
            case BatchedTimesheetTierStatus.InProgress:
                switch (next)
                {
                    case BatchedTimesheetTierStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedTimesheetTierStatus.Completed:
                switch (next)
                {
                    case BatchedTimesheetTierStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedTimesheetTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched timesheet tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Minimum * Maximum, 2);
    }

    public BatchedTimesheetTier Copy()
    {
        return new BatchedTimesheetTier(
            Code,
            Label,
            Minimum,
            Approved,
            Maximum,
            Score,
            Currency,
            Visible,
            Reviewer,
            Locale,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
