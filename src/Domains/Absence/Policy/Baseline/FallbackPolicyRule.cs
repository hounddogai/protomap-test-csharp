namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback policy rule is in its life cycle.</summary>
public enum FallbackPolicyRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One fallback policy rule, with what the planners need to act on it.</summary>
public sealed class FallbackPolicyRule(string code, string title, double floorValue, double maximum, string grade, string contactEmail, double rate, List<string> tags, FallbackPolicyRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public string ContactEmail { get; set; } = contactEmail;
    public double Rate { get; set; } = rate;
    public List<string> Tags { get; set; } = tags;
    public FallbackPolicyRuleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback policy rule in status planned.</summary>
    public static FallbackPolicyRule NewFallbackPolicyRule(string code, string title)
    {
        return new FallbackPolicyRule(
            code,
            title,
            0.0,
            0.0,
            "",
            "",
            0.0,
            new List<string>(),
            FallbackPolicyRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPolicyRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPolicyRuleStatus next)
    {
        switch (Status)
        {
            case FallbackPolicyRuleStatus.Planned:
                return next == FallbackPolicyRuleStatus.Completed;
            case FallbackPolicyRuleStatus.Scheduled:
                switch (next)
                {
                    case FallbackPolicyRuleStatus.InProgress:
                        return true;
                }
                return false;
            case FallbackPolicyRuleStatus.InProgress:
                switch (next)
                {
                    case FallbackPolicyRuleStatus.Completed:
                        return true;
                }
                return false;
            case FallbackPolicyRuleStatus.Completed:
                return next == FallbackPolicyRuleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackPolicyRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback policy rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback policy rule"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Maximum * FloorValue, 2);
    }

    public FallbackPolicyRule Copy()
    {
        return new FallbackPolicyRule(Code, Title, FloorValue, Maximum, Grade, ContactEmail, Rate, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
