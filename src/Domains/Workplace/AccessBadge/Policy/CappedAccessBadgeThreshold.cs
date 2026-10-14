namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped access badge threshold is in its life cycle.</summary>
public enum CappedAccessBadgeThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped access badge threshold. Its code is unique within a tenant.</summary>
public sealed class CappedAccessBadgeThreshold(string code, string title, string reviewer, Dictionary<string, string> attributes, string grade, int priority, CappedAccessBadgeThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public CappedAccessBadgeThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a capped access badge threshold in status new.</summary>
    public static CappedAccessBadgeThreshold NewCappedAccessBadgeThreshold(string code, string title)
    {
        return new CappedAccessBadgeThreshold(
            code,
            title,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            CappedAccessBadgeThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAccessBadgeThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAccessBadgeThresholdStatus next)
    {
        switch (Status)
        {
            case CappedAccessBadgeThresholdStatus.New:
                switch (next)
                {
                    case CappedAccessBadgeThresholdStatus.Failed:
                        return true;
                }
                return false;
            case CappedAccessBadgeThresholdStatus.Queued:
                return next == CappedAccessBadgeThresholdStatus.Failed;
            case CappedAccessBadgeThresholdStatus.Running:
                return next == CappedAccessBadgeThresholdStatus.Done || next == CappedAccessBadgeThresholdStatus.Failed;
            case CappedAccessBadgeThresholdStatus.Done:
                switch (next)
                {
                    case CappedAccessBadgeThresholdStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAccessBadgeThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped access badge threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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
        return (double)Priority;
    }

    public CappedAccessBadgeThreshold Copy()
    {
        return new CappedAccessBadgeThreshold(Code, Title, Reviewer, Attributes, Grade, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
