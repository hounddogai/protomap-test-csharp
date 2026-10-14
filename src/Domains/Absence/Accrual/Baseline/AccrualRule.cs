namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual rule is in its life cycle.</summary>
public enum AccrualRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One accrual rule, with what the planners need to act on it.</summary>
public sealed class AccrualRule(string code, string name, string segment, string phone, string region, int priority, string department, string timezone, AccrualRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public string Phone { get; set; } = phone;
    public string Region { get; set; } = region;
    public int Priority { get; set; } = priority;
    public string Department { get; set; } = department;
    public string Timezone { get; set; } = timezone;
    public AccrualRuleStatus Status { get; set; } = status;

    /// <summary>Starts a accrual rule in status open.</summary>
    public static AccrualRule NewAccrualRule(string code, string name)
    {
        return new AccrualRule(code, name, "", "", "", 0, "", "", AccrualRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualRuleStatus next)
    {
        switch (Status)
        {
            case AccrualRuleStatus.Open:
                switch (next)
                {
                    case AccrualRuleStatus.Resolved:
                    case AccrualRuleStatus.Reopened:
                        return true;
                }
                return false;
            case AccrualRuleStatus.InReview:
                return next == AccrualRuleStatus.Resolved || next == AccrualRuleStatus.Reopened;
            case AccrualRuleStatus.Resolved:
                return next == AccrualRuleStatus.Archived;
            case AccrualRuleStatus.Reopened:
                return next == AccrualRuleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AccrualRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public AccrualRule Copy()
    {
        return new AccrualRule(Code, Name, Segment, Phone, Region, Priority, Department, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
