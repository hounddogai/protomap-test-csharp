namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient desk template is in its life cycle.</summary>
public enum LenientDeskTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One lenient desk template, with what the planners need to act on it.</summary>
public sealed class LenientDeskTemplate(string code, string name, int startsAt, double cost, string jobCode, bool approved, string channel, int leaveDays, string reviewer, Dictionary<string, string> attributes, string contactEmail, LenientDeskTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public double Cost { get; set; } = cost;
    public string JobCode { get; set; } = jobCode;
    public bool Approved { get; set; } = approved;
    public string Channel { get; set; } = channel;
    public int LeaveDays { get; set; } = leaveDays;
    public string Reviewer { get; set; } = reviewer;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ContactEmail { get; set; } = contactEmail;
    public LenientDeskTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a lenient desk template in status open.</summary>
    public static LenientDeskTemplate NewLenientDeskTemplate(string code, string name)
    {
        return new LenientDeskTemplate(
            code,
            name,
            0,
            0.0,
            "",
            false,
            "",
            0,
            "",
            new Dictionary<string, string>(),
            "",
            LenientDeskTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeskTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeskTemplateStatus next)
    {
        switch (Status)
        {
            case LenientDeskTemplateStatus.Open:
                return next == LenientDeskTemplateStatus.Resolved || next == LenientDeskTemplateStatus.Reopened;
            case LenientDeskTemplateStatus.InReview:
                return next == LenientDeskTemplateStatus.Resolved;
            case LenientDeskTemplateStatus.Resolved:
                return next == LenientDeskTemplateStatus.Reopened || next == LenientDeskTemplateStatus.Archived;
            case LenientDeskTemplateStatus.Reopened:
                return next == LenientDeskTemplateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientDeskTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient desk template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
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

    public double AdjustedLoad()
    {
        return Math.Round((double)LeaveDays - (double)StartsAt, 2);
    }

    public LenientDeskTemplate Copy()
    {
        return new LenientDeskTemplate(
            Code,
            Name,
            StartsAt,
            Cost,
            JobCode,
            Approved,
            Channel,
            LeaveDays,
            Reviewer,
            Attributes,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
