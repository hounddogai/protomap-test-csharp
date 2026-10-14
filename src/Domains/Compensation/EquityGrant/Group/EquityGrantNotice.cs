namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant notice is in its life cycle.</summary>
public enum EquityGrantNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One equity grant notice, with what the planners need to act on it.</summary>
public sealed class EquityGrantNotice(string code, string title, int capacity, string timezone, int version, double budget, EquityGrantNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Timezone { get; set; } = timezone;
    public int Version { get; set; } = version;
    public double Budget { get; set; } = budget;
    public EquityGrantNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant notice in status open.</summary>
    public static EquityGrantNotice NewEquityGrantNotice(string code, string title)
    {
        return new EquityGrantNotice(code, title, 0, "", 0, 0.0, EquityGrantNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantNoticeStatus next)
    {
        switch (Status)
        {
            case EquityGrantNoticeStatus.Open:
                switch (next)
                {
                    case EquityGrantNoticeStatus.InReview:
                    case EquityGrantNoticeStatus.Resolved:
                        return true;
                }
                return false;
            case EquityGrantNoticeStatus.InReview:
                switch (next)
                {
                    case EquityGrantNoticeStatus.Resolved:
                    case EquityGrantNoticeStatus.Reopened:
                        return true;
                }
                return false;
            case EquityGrantNoticeStatus.Resolved:
                switch (next)
                {
                    case EquityGrantNoticeStatus.Reopened:
                    case EquityGrantNoticeStatus.Archived:
                        return true;
                }
                return false;
            case EquityGrantNoticeStatus.Reopened:
                switch (next)
                {
                    case EquityGrantNoticeStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"equity grant notice"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Version - Budget, 2);
    }

    public EquityGrantNotice Copy()
    {
        return new EquityGrantNotice(Code, Title, Capacity, Timezone, Version, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
