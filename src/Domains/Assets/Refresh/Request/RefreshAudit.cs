namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh audit is in its life cycle.</summary>
public enum RefreshAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One refresh audit, with what the planners need to act on it.</summary>
public sealed class RefreshAudit(string code, string title, List<string> tags, double hours, string currency, string summary, RefreshAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public double Hours { get; set; } = hours;
    public string Currency { get; set; } = currency;
    public string Summary { get; set; } = summary;
    public RefreshAuditStatus Status { get; set; } = status;

    /// <summary>Starts a refresh audit in status created.</summary>
    public static RefreshAudit NewRefreshAudit(string code, string title)
    {
        return new RefreshAudit(code, title, new List<string>(), 0.0, "", "", RefreshAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshAuditStatus next)
    {
        switch (Status)
        {
            case RefreshAuditStatus.Created:
                switch (next)
                {
                    case RefreshAuditStatus.Validated:
                        return true;
                }
                return false;
            case RefreshAuditStatus.Validated:
                switch (next)
                {
                    case RefreshAuditStatus.Published:
                    case RefreshAuditStatus.Retired:
                        return true;
                }
                return false;
            case RefreshAuditStatus.Published:
                return next == RefreshAuditStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RefreshAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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
        return Hours;
    }

    public RefreshAudit Copy()
    {
        return new RefreshAudit(Code, Title, Tags, Hours, Currency, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
