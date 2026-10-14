namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet tier is in its life cycle.</summary>
public enum TimesheetTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a timesheet tier and its current status.</summary>
public sealed class TimesheetTier(string code, string title, int retries, string costCenter, double ratio, double weight, string checksum, string source, string currency, TimesheetTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string CostCenter { get; set; } = costCenter;
    public double Ratio { get; set; } = ratio;
    public double Weight { get; set; } = weight;
    public string Checksum { get; set; } = checksum;
    public string Source { get; set; } = source;
    public string Currency { get; set; } = currency;
    public TimesheetTierStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet tier in status idle.</summary>
    public static TimesheetTier NewTimesheetTier(string code, string title)
    {
        return new TimesheetTier(code, title, 0, "", 0.0, 0.0, "", "", "", TimesheetTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetTierStatus next)
    {
        switch (Status)
        {
            case TimesheetTierStatus.Idle:
                switch (next)
                {
                    case TimesheetTierStatus.Draining:
                        return true;
                }
                return false;
            case TimesheetTierStatus.Warming:
                switch (next)
                {
                    case TimesheetTierStatus.Draining:
                        return true;
                }
                return false;
            case TimesheetTierStatus.Ready:
                switch (next)
                {
                    case TimesheetTierStatus.Draining:
                        return true;
                }
                return false;
            case TimesheetTierStatus.Draining:
                switch (next)
                {
                    case TimesheetTierStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round(Ratio - (double)Retries, 2);
    }

    public TimesheetTier Copy()
    {
        return new TimesheetTier(
            Code,
            Title,
            Retries,
            CostCenter,
            Ratio,
            Weight,
            Checksum,
            Source,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
