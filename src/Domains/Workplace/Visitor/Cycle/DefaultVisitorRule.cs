namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default visitor rule is in its life cycle.</summary>
public enum DefaultVisitorRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a default visitor rule and its current status.</summary>
public sealed class DefaultVisitorRule(string code, string label, int priority, string name, string employeeId, string segment, int attempts, int graceDays, int capacity, int startsAt, double score, DefaultVisitorRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string Segment { get; set; } = segment;
    public int Attempts { get; set; } = attempts;
    public int GraceDays { get; set; } = graceDays;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public double Score { get; set; } = score;
    public DefaultVisitorRuleStatus Status { get; set; } = status;

    /// <summary>Starts a default visitor rule in status open.</summary>
    public static DefaultVisitorRule NewDefaultVisitorRule(string code, string label)
    {
        return new DefaultVisitorRule(code, label, 0, "", "", "", 0, 0, 0, 0, 0.0, DefaultVisitorRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultVisitorRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultVisitorRuleStatus next)
    {
        switch (Status)
        {
            case DefaultVisitorRuleStatus.Open:
                return next == DefaultVisitorRuleStatus.InReview;
            case DefaultVisitorRuleStatus.InReview:
                switch (next)
                {
                    case DefaultVisitorRuleStatus.Reopened:
                        return true;
                }
                return false;
            case DefaultVisitorRuleStatus.Resolved:
                return next == DefaultVisitorRuleStatus.Reopened || next == DefaultVisitorRuleStatus.Archived;
            case DefaultVisitorRuleStatus.Reopened:
                switch (next)
                {
                    case DefaultVisitorRuleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultVisitorRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default visitor rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Attempts - Score, 2);
    }

    public DefaultVisitorRule Copy()
    {
        return new DefaultVisitorRule(
            Code,
            Label,
            Priority,
            Name,
            EmployeeId,
            Segment,
            Attempts,
            GraceDays,
            Capacity,
            StartsAt,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
