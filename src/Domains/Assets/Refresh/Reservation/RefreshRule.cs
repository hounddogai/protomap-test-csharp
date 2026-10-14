namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh rule is in its life cycle.</summary>
public enum RefreshRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A refresh rule. Its code is unique within a tenant.</summary>
public sealed class RefreshRule(string code, string label, string ownerId, string employeeId, List<string> tags, string source, string externalRef, int retries, RefreshRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public string Source { get; set; } = source;
    public string ExternalRef { get; set; } = externalRef;
    public int Retries { get; set; } = retries;
    public RefreshRuleStatus Status { get; set; } = status;

    /// <summary>Starts a refresh rule in status open.</summary>
    public static RefreshRule NewRefreshRule(string code, string label)
    {
        return new RefreshRule(code, label, "", "", new List<string>(), "", "", 0, RefreshRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshRuleStatus next)
    {
        switch (Status)
        {
            case RefreshRuleStatus.Open:
                switch (next)
                {
                    case RefreshRuleStatus.InReview:
                    case RefreshRuleStatus.Reopened:
                        return true;
                }
                return false;
            case RefreshRuleStatus.InReview:
                switch (next)
                {
                    case RefreshRuleStatus.Archived:
                        return true;
                }
                return false;
            case RefreshRuleStatus.Resolved:
                switch (next)
                {
                    case RefreshRuleStatus.Reopened:
                    case RefreshRuleStatus.Archived:
                        return true;
                }
                return false;
            case RefreshRuleStatus.Reopened:
                return next == RefreshRuleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RefreshRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public RefreshRule Copy()
    {
        return new RefreshRule(Code, Label, OwnerId, EmployeeId, Tags, Source, ExternalRef, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
