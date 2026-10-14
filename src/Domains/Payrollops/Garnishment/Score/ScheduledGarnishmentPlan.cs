namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled garnishment plan is in its life cycle.</summary>
public enum ScheduledGarnishmentPlanStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scheduled garnishment plan, with what the planners need to act on it.</summary>
public sealed class ScheduledGarnishmentPlan(string code, string title, double weight, List<string> tags, string salaryBand, int version, string channel, ScheduledGarnishmentPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public List<string> Tags { get; set; } = tags;
    public string SalaryBand { get; set; } = salaryBand;
    public int Version { get; set; } = version;
    public string Channel { get; set; } = channel;
    public ScheduledGarnishmentPlanStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled garnishment plan in status proposed.</summary>
    public static ScheduledGarnishmentPlan NewScheduledGarnishmentPlan(string code, string title)
    {
        return new ScheduledGarnishmentPlan(
            code,
            title,
            0.0,
            new List<string>(),
            "",
            0,
            "",
            ScheduledGarnishmentPlanStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledGarnishmentPlanStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledGarnishmentPlanStatus next)
    {
        switch (Status)
        {
            case ScheduledGarnishmentPlanStatus.Proposed:
                return next == ScheduledGarnishmentPlanStatus.Rejected || next == ScheduledGarnishmentPlanStatus.Withdrawn;
            case ScheduledGarnishmentPlanStatus.Approved:
                return next == ScheduledGarnishmentPlanStatus.Rejected || next == ScheduledGarnishmentPlanStatus.Withdrawn;
            case ScheduledGarnishmentPlanStatus.Rejected:
                switch (next)
                {
                    case ScheduledGarnishmentPlanStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledGarnishmentPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled garnishment plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"scheduled garnishment plan"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedValue()
    {
        return Math.Round(Weight * (double)Version, 2);
    }

    public ScheduledGarnishmentPlan Copy()
    {
        return new ScheduledGarnishmentPlan(Code, Title, Weight, Tags, SalaryBand, Version, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
