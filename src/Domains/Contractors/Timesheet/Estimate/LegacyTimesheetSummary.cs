namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy timesheet summary is in its life cycle.</summary>
public enum LegacyTimesheetSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a legacy timesheet summary and its current status.</summary>
public sealed class LegacyTimesheetSummary(string code, string label, int priority, string costCenter, string badge, double rate, string notes, string phone, string country, string managerId, LegacyTimesheetSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string CostCenter { get; set; } = costCenter;
    public string Badge { get; set; } = badge;
    public double Rate { get; set; } = rate;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public string Country { get; set; } = country;
    public string ManagerId { get; set; } = managerId;
    public LegacyTimesheetSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a legacy timesheet summary in status new.</summary>
    public static LegacyTimesheetSummary NewLegacyTimesheetSummary(string code, string label)
    {
        return new LegacyTimesheetSummary(
            code,
            label,
            0,
            "",
            "",
            0.0,
            "",
            "",
            "",
            "",
            LegacyTimesheetSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTimesheetSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTimesheetSummaryStatus next)
    {
        switch (Status)
        {
            case LegacyTimesheetSummaryStatus.New:
                switch (next)
                {
                    case LegacyTimesheetSummaryStatus.Done:
                        return true;
                }
                return false;
            case LegacyTimesheetSummaryStatus.Queued:
                return next == LegacyTimesheetSummaryStatus.Running || next == LegacyTimesheetSummaryStatus.Done;
            case LegacyTimesheetSummaryStatus.Running:
                switch (next)
                {
                    case LegacyTimesheetSummaryStatus.Done:
                        return true;
                }
                return false;
            case LegacyTimesheetSummaryStatus.Done:
                return next == LegacyTimesheetSummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyTimesheetSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy timesheet summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Priority * Rate, 2);
    }

    public LegacyTimesheetSummary Copy()
    {
        return new LegacyTimesheetSummary(
            Code,
            Label,
            Priority,
            CostCenter,
            Badge,
            Rate,
            Notes,
            Phone,
            Country,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
