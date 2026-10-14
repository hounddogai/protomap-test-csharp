namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement audit is in its life cycle.</summary>
public enum EngagementAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A engagement audit. Its code is unique within a tenant.</summary>
public sealed class EngagementAudit(string code, string name, Dictionary<string, string> attributes, List<string> tags, string employeeId, string label, double score, EngagementAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public List<string> Tags { get; set; } = tags;
    public string EmployeeId { get; set; } = employeeId;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public EngagementAuditStatus Status { get; set; } = status;

    /// <summary>Starts a engagement audit in status open.</summary>
    public static EngagementAudit NewEngagementAudit(string code, string name)
    {
        return new EngagementAudit(
            code,
            name,
            new Dictionary<string, string>(),
            new List<string>(),
            "",
            "",
            0.0,
            EngagementAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementAuditStatus next)
    {
        switch (Status)
        {
            case EngagementAuditStatus.Open:
                return next == EngagementAuditStatus.Resolved;
            case EngagementAuditStatus.InReview:
                return next == EngagementAuditStatus.Resolved;
            case EngagementAuditStatus.Resolved:
                switch (next)
                {
                    case EngagementAuditStatus.Archived:
                        return true;
                }
                return false;
            case EngagementAuditStatus.Reopened:
                switch (next)
                {
                    case EngagementAuditStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
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

    public double MeasuredValue()
    {
        return Score;
    }

    public EngagementAudit Copy()
    {
        return new EngagementAudit(Code, Name, Attributes, Tags, EmployeeId, Label, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
