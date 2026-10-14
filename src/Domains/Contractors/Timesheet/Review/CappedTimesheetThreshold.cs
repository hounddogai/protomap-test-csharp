namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped timesheet threshold is in its life cycle.</summary>
public enum CappedTimesheetThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a capped timesheet threshold and its current status.</summary>
public sealed class CappedTimesheetThreshold(string code, string name, double maximum, double threshold, string description, bool visible, bool active, double ceiling, CappedTimesheetThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public double Threshold { get; set; } = threshold;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public bool Active { get; set; } = active;
    public double Ceiling { get; set; } = ceiling;
    public CappedTimesheetThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a capped timesheet threshold in status requested.</summary>
    public static CappedTimesheetThreshold NewCappedTimesheetThreshold(string code, string name)
    {
        return new CappedTimesheetThreshold(
            code,
            name,
            0.0,
            0.0,
            "",
            false,
            false,
            0.0,
            CappedTimesheetThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedTimesheetThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedTimesheetThresholdStatus next)
    {
        switch (Status)
        {
            case CappedTimesheetThresholdStatus.Requested:
                switch (next)
                {
                    case CappedTimesheetThresholdStatus.Expired:
                        return true;
                }
                return false;
            case CappedTimesheetThresholdStatus.Granted:
                switch (next)
                {
                    case CappedTimesheetThresholdStatus.Revoked:
                    case CappedTimesheetThresholdStatus.Expired:
                        return true;
                }
                return false;
            case CappedTimesheetThresholdStatus.Revoked:
                switch (next)
                {
                    case CappedTimesheetThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedTimesheetThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped timesheet threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped timesheet threshold"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Ceiling * Maximum, 2);
    }

    public CappedTimesheetThreshold Copy()
    {
        return new CappedTimesheetThreshold(
            Code,
            Name,
            Maximum,
            Threshold,
            Description,
            Visible,
            Active,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
