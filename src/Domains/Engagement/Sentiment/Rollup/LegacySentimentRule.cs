namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy sentiment rule is in its life cycle.</summary>
public enum LegacySentimentRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One legacy sentiment rule, with what the planners need to act on it.</summary>
public sealed class LegacySentimentRule(string code, string title, bool active, int batchSize, string locale, double discount, string label, double ratio, LegacySentimentRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public int BatchSize { get; set; } = batchSize;
    public string Locale { get; set; } = locale;
    public double Discount { get; set; } = discount;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public LegacySentimentRuleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy sentiment rule in status proposed.</summary>
    public static LegacySentimentRule NewLegacySentimentRule(string code, string title)
    {
        return new LegacySentimentRule(code, title, false, 0, "", 0.0, "", 0.0, LegacySentimentRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySentimentRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySentimentRuleStatus next)
    {
        switch (Status)
        {
            case LegacySentimentRuleStatus.Proposed:
                switch (next)
                {
                    case LegacySentimentRuleStatus.Rejected:
                    case LegacySentimentRuleStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacySentimentRuleStatus.Approved:
                return next == LegacySentimentRuleStatus.Rejected || next == LegacySentimentRuleStatus.Withdrawn;
            case LegacySentimentRuleStatus.Rejected:
                switch (next)
                {
                    case LegacySentimentRuleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacySentimentRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy sentiment rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy sentiment rule"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)BatchSize * Discount, 2);
    }

    public LegacySentimentRule Copy()
    {
        return new LegacySentimentRule(Code, Title, Active, BatchSize, Locale, Discount, Label, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
