namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default balance draft is in its life cycle.</summary>
public enum DefaultBalanceDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default balance draft as the balance team keeps it.</summary>
public sealed class DefaultBalanceDraft(string code, string title, Dictionary<string, string> attributes, string name, int leaveDays, string summary, DefaultBalanceDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string Summary { get; set; } = summary;
    public DefaultBalanceDraftStatus Status { get; set; } = status;

    /// <summary>Starts a default balance draft in status draft.</summary>
    public static DefaultBalanceDraft NewDefaultBalanceDraft(string code, string title)
    {
        return new DefaultBalanceDraft(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            DefaultBalanceDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBalanceDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBalanceDraftStatus next)
    {
        switch (Status)
        {
            case DefaultBalanceDraftStatus.Draft:
                return next == DefaultBalanceDraftStatus.Active;
            case DefaultBalanceDraftStatus.Pending:
                switch (next)
                {
                    case DefaultBalanceDraftStatus.Closed:
                        return true;
                }
                return false;
            case DefaultBalanceDraftStatus.Active:
                return next == DefaultBalanceDraftStatus.Suspended || next == DefaultBalanceDraftStatus.Closed;
            case DefaultBalanceDraftStatus.Suspended:
                switch (next)
                {
                    case DefaultBalanceDraftStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultBalanceDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default balance draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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
        return (double)LeaveDays;
    }

    public DefaultBalanceDraft Copy()
    {
        return new DefaultBalanceDraft(Code, Title, Attributes, Name, LeaveDays, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
