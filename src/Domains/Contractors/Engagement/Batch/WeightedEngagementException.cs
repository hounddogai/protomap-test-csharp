namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted engagement exception is in its life cycle.</summary>
public enum WeightedEngagementExceptionStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One weighted engagement exception, with what the planners need to act on it.</summary>
public sealed class WeightedEngagementException(string code, string name, double discount, bool archived, string timezone, int priority, double fte, WeightedEngagementExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public bool Archived { get; set; } = archived;
    public string Timezone { get; set; } = timezone;
    public int Priority { get; set; } = priority;
    public double Fte { get; set; } = fte;
    public WeightedEngagementExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a weighted engagement exception in status open.</summary>
    public static WeightedEngagementException NewWeightedEngagementException(string code, string name)
    {
        return new WeightedEngagementException(
            code,
            name,
            0.0,
            false,
            "",
            0,
            0.0,
            WeightedEngagementExceptionStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedEngagementExceptionStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedEngagementExceptionStatus next)
    {
        switch (Status)
        {
            case WeightedEngagementExceptionStatus.Open:
                return next == WeightedEngagementExceptionStatus.InReview || next == WeightedEngagementExceptionStatus.Reopened;
            case WeightedEngagementExceptionStatus.InReview:
                return next == WeightedEngagementExceptionStatus.Reopened || next == WeightedEngagementExceptionStatus.Archived;
            case WeightedEngagementExceptionStatus.Resolved:
                switch (next)
                {
                    case WeightedEngagementExceptionStatus.Reopened:
                    case WeightedEngagementExceptionStatus.Archived:
                        return true;
                }
                return false;
            case WeightedEngagementExceptionStatus.Reopened:
                return next == WeightedEngagementExceptionStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedEngagementExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted engagement exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount * Fte, 2);
    }

    public WeightedEngagementException Copy()
    {
        return new WeightedEngagementException(Code, Name, Discount, Archived, Timezone, Priority, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
