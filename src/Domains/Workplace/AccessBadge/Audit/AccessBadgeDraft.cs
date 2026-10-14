namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge draft is in its life cycle.</summary>
public enum AccessBadgeDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a access badge draft and its current status.</summary>
public sealed class AccessBadgeDraft(string code, string title, int updatedAt, string source, string skill, double ratio, string summary, string category, int capacity, string description, AccessBadgeDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Source { get; set; } = source;
    public string Skill { get; set; } = skill;
    public double Ratio { get; set; } = ratio;
    public string Summary { get; set; } = summary;
    public string Category { get; set; } = category;
    public int Capacity { get; set; } = capacity;
    public string Description { get; set; } = description;
    public AccessBadgeDraftStatus Status { get; set; } = status;

    /// <summary>Starts a access badge draft in status proposed.</summary>
    public static AccessBadgeDraft NewAccessBadgeDraft(string code, string title)
    {
        return new AccessBadgeDraft(code, title, 0, "", "", 0.0, "", "", 0, "", AccessBadgeDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeDraftStatus next)
    {
        switch (Status)
        {
            case AccessBadgeDraftStatus.Proposed:
                return next == AccessBadgeDraftStatus.Approved || next == AccessBadgeDraftStatus.Rejected;
            case AccessBadgeDraftStatus.Approved:
                switch (next)
                {
                    case AccessBadgeDraftStatus.Rejected:
                        return true;
                }
                return false;
            case AccessBadgeDraftStatus.Rejected:
                return next == AccessBadgeDraftStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AccessBadgeDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)UpdatedAt - Ratio, 2);
    }

    public AccessBadgeDraft Copy()
    {
        return new AccessBadgeDraft(
            Code,
            Title,
            UpdatedAt,
            Source,
            Skill,
            Ratio,
            Summary,
            Category,
            Capacity,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
