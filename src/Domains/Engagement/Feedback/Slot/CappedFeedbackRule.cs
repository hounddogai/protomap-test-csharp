namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped feedback rule is in its life cycle.</summary>
public enum CappedFeedbackRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A capped feedback rule as the feedback team keeps it.</summary>
public sealed class CappedFeedbackRule(string code, string label, double ceiling, string reviewer, string team, string timezone, int tenureMonths, string locale, CappedFeedbackRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public string Reviewer { get; set; } = reviewer;
    public string Team { get; set; } = team;
    public string Timezone { get; set; } = timezone;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Locale { get; set; } = locale;
    public CappedFeedbackRuleStatus Status { get; set; } = status;

    /// <summary>Starts a capped feedback rule in status draft.</summary>
    public static CappedFeedbackRule NewCappedFeedbackRule(string code, string label)
    {
        return new CappedFeedbackRule(code, label, 0.0, "", "", "", 0, "", CappedFeedbackRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedFeedbackRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedFeedbackRuleStatus next)
    {
        switch (Status)
        {
            case CappedFeedbackRuleStatus.Draft:
                return next == CappedFeedbackRuleStatus.Active || next == CappedFeedbackRuleStatus.Suspended;
            case CappedFeedbackRuleStatus.Pending:
                switch (next)
                {
                    case CappedFeedbackRuleStatus.Closed:
                        return true;
                }
                return false;
            case CappedFeedbackRuleStatus.Active:
                switch (next)
                {
                    case CappedFeedbackRuleStatus.Closed:
                        return true;
                }
                return false;
            case CappedFeedbackRuleStatus.Suspended:
                return next == CappedFeedbackRuleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedFeedbackRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped feedback rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ceiling * (double)TenureMonths, 2);
    }

    public CappedFeedbackRule Copy()
    {
        return new CappedFeedbackRule(Code, Label, Ceiling, Reviewer, Team, Timezone, TenureMonths, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
