namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional exception audit is in its life cycle.</summary>
public enum RegionalExceptionAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One regional exception audit, with what the planners need to act on it.</summary>
public sealed class RegionalExceptionAudit(string code, string name, int expiresAt, double rate, Dictionary<string, string> attributes, string label, RegionalExceptionAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Rate { get; set; } = rate;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Label { get; set; } = label;
    public RegionalExceptionAuditStatus Status { get; set; } = status;

    /// <summary>Starts a regional exception audit in status open.</summary>
    public static RegionalExceptionAudit NewRegionalExceptionAudit(string code, string name)
    {
        return new RegionalExceptionAudit(
            code,
            name,
            0,
            0.0,
            new Dictionary<string, string>(),
            "",
            RegionalExceptionAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalExceptionAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalExceptionAuditStatus next)
    {
        switch (Status)
        {
            case RegionalExceptionAuditStatus.Open:
                switch (next)
                {
                    case RegionalExceptionAuditStatus.InReview:
                    case RegionalExceptionAuditStatus.Resolved:
                        return true;
                }
                return false;
            case RegionalExceptionAuditStatus.InReview:
                return next == RegionalExceptionAuditStatus.Resolved || next == RegionalExceptionAuditStatus.Reopened;
            case RegionalExceptionAuditStatus.Resolved:
                switch (next)
                {
                    case RegionalExceptionAuditStatus.Reopened:
                    case RegionalExceptionAuditStatus.Archived:
                        return true;
                }
                return false;
            case RegionalExceptionAuditStatus.Reopened:
                switch (next)
                {
                    case RegionalExceptionAuditStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalExceptionAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional exception audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedLoad()
    {
        return Math.Round((double)ExpiresAt - Rate, 2);
    }

    public RegionalExceptionAudit Copy()
    {
        return new RegionalExceptionAudit(Code, Name, ExpiresAt, Rate, Attributes, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
