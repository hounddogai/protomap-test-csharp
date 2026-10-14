namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default desk score is in its life cycle.</summary>
public enum DefaultDeskScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a default desk score and its current status.</summary>
public sealed class DefaultDeskScore(string code, string label, string contactEmail, int graceDays, double minimum, bool active, string employeeId, Dictionary<string, string> attributes, string locale, string segment, int version, DefaultDeskScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public int GraceDays { get; set; } = graceDays;
    public double Minimum { get; set; } = minimum;
    public bool Active { get; set; } = active;
    public string EmployeeId { get; set; } = employeeId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Locale { get; set; } = locale;
    public string Segment { get; set; } = segment;
    public int Version { get; set; } = version;
    public DefaultDeskScoreStatus Status { get; set; } = status;

    /// <summary>Starts a default desk score in status open.</summary>
    public static DefaultDeskScore NewDefaultDeskScore(string code, string label)
    {
        return new DefaultDeskScore(
            code,
            label,
            "",
            0,
            0.0,
            false,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0,
            DefaultDeskScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultDeskScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultDeskScoreStatus next)
    {
        switch (Status)
        {
            case DefaultDeskScoreStatus.Open:
                return next == DefaultDeskScoreStatus.InReview || next == DefaultDeskScoreStatus.Resolved;
            case DefaultDeskScoreStatus.InReview:
                switch (next)
                {
                    case DefaultDeskScoreStatus.Resolved:
                    case DefaultDeskScoreStatus.Reopened:
                        return true;
                }
                return false;
            case DefaultDeskScoreStatus.Resolved:
                return next == DefaultDeskScoreStatus.Reopened;
            case DefaultDeskScoreStatus.Reopened:
                switch (next)
                {
                    case DefaultDeskScoreStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultDeskScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default desk score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"default desk score"} {Code}";
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

    public double EffectiveLoad()
    {
        return Math.Round((double)Version + Minimum, 2);
    }

    public DefaultDeskScore Copy()
    {
        return new DefaultDeskScore(
            Code,
            Label,
            ContactEmail,
            GraceDays,
            Minimum,
            Active,
            EmployeeId,
            Attributes,
            Locale,
            Segment,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
