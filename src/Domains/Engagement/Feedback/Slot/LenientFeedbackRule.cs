namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient feedback rule is in its life cycle.</summary>
public enum LenientFeedbackRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient feedback rule. Its code is unique within a tenant.</summary>
public sealed class LenientFeedbackRule(string code, string label, string timezone, string channel, int windowDays, LenientFeedbackRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public string Channel { get; set; } = channel;
    public int WindowDays { get; set; } = windowDays;
    public LenientFeedbackRuleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient feedback rule in status proposed.</summary>
    public static LenientFeedbackRule NewLenientFeedbackRule(string code, string label)
    {
        return new LenientFeedbackRule(code, label, "", "", 0, LenientFeedbackRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientFeedbackRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientFeedbackRuleStatus next)
    {
        switch (Status)
        {
            case LenientFeedbackRuleStatus.Proposed:
                return next == LenientFeedbackRuleStatus.Rejected;
            case LenientFeedbackRuleStatus.Approved:
                switch (next)
                {
                    case LenientFeedbackRuleStatus.Rejected:
                        return true;
                }
                return false;
            case LenientFeedbackRuleStatus.Rejected:
                switch (next)
                {
                    case LenientFeedbackRuleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientFeedbackRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient feedback rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public LenientFeedbackRule Copy()
    {
        return new LenientFeedbackRule(Code, Label, Timezone, Channel, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
