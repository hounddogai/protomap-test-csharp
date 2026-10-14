namespace Acme.Workforce.Engagement.ActionPlan.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default action plan policy is in its life cycle.</summary>
public enum DefaultActionPlanPolicyStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a default action plan policy and its current status.</summary>
public sealed class DefaultActionPlanPolicy(string code, string label, bool visible, int durationMinutes, double ratio, string currency, string category, double hours, double fte, string notes, int priority, DefaultActionPlanPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Ratio { get; set; } = ratio;
    public string Currency { get; set; } = currency;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public double Fte { get; set; } = fte;
    public string Notes { get; set; } = notes;
    public int Priority { get; set; } = priority;
    public DefaultActionPlanPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a default action plan policy in status draft.</summary>
    public static DefaultActionPlanPolicy NewDefaultActionPlanPolicy(string code, string label)
    {
        return new DefaultActionPlanPolicy(
            code,
            label,
            false,
            0,
            0.0,
            "",
            "",
            0.0,
            0.0,
            "",
            0,
            DefaultActionPlanPolicyStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultActionPlanPolicyStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultActionPlanPolicyStatus next)
    {
        switch (Status)
        {
            case DefaultActionPlanPolicyStatus.Draft:
                return next == DefaultActionPlanPolicyStatus.Active;
            case DefaultActionPlanPolicyStatus.Pending:
                switch (next)
                {
                    case DefaultActionPlanPolicyStatus.Suspended:
                    case DefaultActionPlanPolicyStatus.Closed:
                        return true;
                }
                return false;
            case DefaultActionPlanPolicyStatus.Active:
                switch (next)
                {
                    case DefaultActionPlanPolicyStatus.Closed:
                        return true;
                }
                return false;
            case DefaultActionPlanPolicyStatus.Suspended:
                switch (next)
                {
                    case DefaultActionPlanPolicyStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultActionPlanPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default action plan policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default action plan policy"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes * Ratio, 2);
    }

    public DefaultActionPlanPolicy Copy()
    {
        return new DefaultActionPlanPolicy(
            Code,
            Label,
            Visible,
            DurationMinutes,
            Ratio,
            Currency,
            Category,
            Hours,
            Fte,
            Notes,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
