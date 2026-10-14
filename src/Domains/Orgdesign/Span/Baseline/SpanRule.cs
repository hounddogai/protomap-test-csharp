namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span rule is in its life cycle.</summary>
public enum SpanRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A span rule. Its code is unique within a tenant.</summary>
public sealed class SpanRule(string code, string title, bool archived, string description, int attempts, string department, SpanRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Description { get; set; } = description;
    public int Attempts { get; set; } = attempts;
    public string Department { get; set; } = department;
    public SpanRuleStatus Status { get; set; } = status;

    /// <summary>Starts a span rule in status requested.</summary>
    public static SpanRule NewSpanRule(string code, string title)
    {
        return new SpanRule(code, title, false, "", 0, "", SpanRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanRuleStatus next)
    {
        switch (Status)
        {
            case SpanRuleStatus.Requested:
                return next == SpanRuleStatus.Expired;
            case SpanRuleStatus.Granted:
                return next == SpanRuleStatus.Revoked || next == SpanRuleStatus.Expired;
            case SpanRuleStatus.Revoked:
                switch (next)
                {
                    case SpanRuleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public SpanRule Copy()
    {
        return new SpanRule(Code, Title, Archived, Description, Attempts, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
