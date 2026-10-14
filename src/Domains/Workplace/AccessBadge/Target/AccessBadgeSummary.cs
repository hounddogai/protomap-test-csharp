namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge summary is in its life cycle.</summary>
public enum AccessBadgeSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A access badge summary. Its code is unique within a tenant.</summary>
public sealed class AccessBadgeSummary(string code, string name, double fte, List<string> tags, int leaveDays, bool active, AccessBadgeSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public List<string> Tags { get; set; } = tags;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Active { get; set; } = active;
    public AccessBadgeSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a access badge summary in status idle.</summary>
    public static AccessBadgeSummary NewAccessBadgeSummary(string code, string name)
    {
        return new AccessBadgeSummary(code, name, 0.0, new List<string>(), 0, false, AccessBadgeSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeSummaryStatus next)
    {
        switch (Status)
        {
            case AccessBadgeSummaryStatus.Idle:
                switch (next)
                {
                    case AccessBadgeSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case AccessBadgeSummaryStatus.Warming:
                return next == AccessBadgeSummaryStatus.Draining || next == AccessBadgeSummaryStatus.Stopped;
            case AccessBadgeSummaryStatus.Ready:
                switch (next)
                {
                    case AccessBadgeSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case AccessBadgeSummaryStatus.Draining:
                return next == AccessBadgeSummaryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AccessBadgeSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double NetLoad()
    {
        return Math.Round((double)LeaveDays + Fte, 2);
    }

    public AccessBadgeSummary Copy()
    {
        return new AccessBadgeSummary(Code, Name, Fte, Tags, LeaveDays, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
