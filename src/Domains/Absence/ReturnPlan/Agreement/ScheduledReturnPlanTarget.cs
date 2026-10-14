namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled return plan target is in its life cycle.</summary>
public enum ScheduledReturnPlanTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled return plan target. Its code is unique within a tenant.</summary>
public sealed class ScheduledReturnPlanTarget(string code, string label, string channel, string department, int graceDays, double hours, string region, string currency, ScheduledReturnPlanTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Department { get; set; } = department;
    public int GraceDays { get; set; } = graceDays;
    public double Hours { get; set; } = hours;
    public string Region { get; set; } = region;
    public string Currency { get; set; } = currency;
    public ScheduledReturnPlanTargetStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled return plan target in status created.</summary>
    public static ScheduledReturnPlanTarget NewScheduledReturnPlanTarget(string code, string label)
    {
        return new ScheduledReturnPlanTarget(
            code,
            label,
            "",
            "",
            0,
            0.0,
            "",
            "",
            ScheduledReturnPlanTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledReturnPlanTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledReturnPlanTargetStatus next)
    {
        switch (Status)
        {
            case ScheduledReturnPlanTargetStatus.Created:
                return next == ScheduledReturnPlanTargetStatus.Validated || next == ScheduledReturnPlanTargetStatus.Retired;
            case ScheduledReturnPlanTargetStatus.Validated:
                switch (next)
                {
                    case ScheduledReturnPlanTargetStatus.Published:
                    case ScheduledReturnPlanTargetStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledReturnPlanTargetStatus.Published:
                switch (next)
                {
                    case ScheduledReturnPlanTargetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledReturnPlanTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled return plan target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled return plan target"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)GraceDays * Hours, 2);
    }

    public ScheduledReturnPlanTarget Copy()
    {
        return new ScheduledReturnPlanTarget(
            Code,
            Label,
            Channel,
            Department,
            GraceDays,
            Hours,
            Region,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
