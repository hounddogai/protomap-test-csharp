namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending equity grant variance is in its life cycle.</summary>
public enum PendingEquityGrantVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One pending equity grant variance, with what the planners need to act on it.</summary>
public sealed class PendingEquityGrantVariance(string code, string name, bool archived, string badge, int attempts, string grade, string title, int capacity, string source, string currency, PendingEquityGrantVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Badge { get; set; } = badge;
    public int Attempts { get; set; } = attempts;
    public string Grade { get; set; } = grade;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Source { get; set; } = source;
    public string Currency { get; set; } = currency;
    public PendingEquityGrantVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a pending equity grant variance in status open.</summary>
    public static PendingEquityGrantVariance NewPendingEquityGrantVariance(string code, string name)
    {
        return new PendingEquityGrantVariance(
            code,
            name,
            false,
            "",
            0,
            "",
            "",
            0,
            "",
            "",
            PendingEquityGrantVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingEquityGrantVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingEquityGrantVarianceStatus next)
    {
        switch (Status)
        {
            case PendingEquityGrantVarianceStatus.Open:
                switch (next)
                {
                    case PendingEquityGrantVarianceStatus.Archived:
                        return true;
                }
                return false;
            case PendingEquityGrantVarianceStatus.InReview:
                return next == PendingEquityGrantVarianceStatus.Reopened;
            case PendingEquityGrantVarianceStatus.Resolved:
                return next == PendingEquityGrantVarianceStatus.Reopened;
            case PendingEquityGrantVarianceStatus.Reopened:
                return next == PendingEquityGrantVarianceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PendingEquityGrantVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending equity grant variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending equity grant variance"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Capacity * (double)Attempts, 2);
    }

    public PendingEquityGrantVariance Copy()
    {
        return new PendingEquityGrantVariance(
            Code,
            Name,
            Archived,
            Badge,
            Attempts,
            Grade,
            Title,
            Capacity,
            Source,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
