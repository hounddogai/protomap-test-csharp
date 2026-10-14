namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan rule is in its life cycle.</summary>
public enum ReturnPlanRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A return plan rule as the return plan team keeps it.</summary>
public sealed class ReturnPlanRule(string code, string label, string category, string channel, string department, string region, int version, string summary, ReturnPlanRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string Channel { get; set; } = channel;
    public string Department { get; set; } = department;
    public string Region { get; set; } = region;
    public int Version { get; set; } = version;
    public string Summary { get; set; } = summary;
    public ReturnPlanRuleStatus Status { get; set; } = status;

    /// <summary>Starts a return plan rule in status planned.</summary>
    public static ReturnPlanRule NewReturnPlanRule(string code, string label)
    {
        return new ReturnPlanRule(code, label, "", "", "", "", 0, "", ReturnPlanRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanRuleStatus next)
    {
        switch (Status)
        {
            case ReturnPlanRuleStatus.Planned:
                return next == ReturnPlanRuleStatus.Cancelled;
            case ReturnPlanRuleStatus.Scheduled:
                switch (next)
                {
                    case ReturnPlanRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case ReturnPlanRuleStatus.InProgress:
                switch (next)
                {
                    case ReturnPlanRuleStatus.Completed:
                    case ReturnPlanRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case ReturnPlanRuleStatus.Completed:
                return next == ReturnPlanRuleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ReturnPlanRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"return plan rule"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public ReturnPlanRule Copy()
    {
        return new ReturnPlanRule(Code, Label, Category, Channel, Department, Region, Version, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
