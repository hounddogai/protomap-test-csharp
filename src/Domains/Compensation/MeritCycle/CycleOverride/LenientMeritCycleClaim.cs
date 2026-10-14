namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient merit cycle claim is in its life cycle.</summary>
public enum LenientMeritCycleClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One lenient merit cycle claim, with what the planners need to act on it.</summary>
public sealed class LenientMeritCycleClaim(string code, string title, bool pinned, string channel, int retries, double rate, LenientMeritCycleClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string Channel { get; set; } = channel;
    public int Retries { get; set; } = retries;
    public double Rate { get; set; } = rate;
    public LenientMeritCycleClaimStatus Status { get; set; } = status;

    /// <summary>Starts a lenient merit cycle claim in status open.</summary>
    public static LenientMeritCycleClaim NewLenientMeritCycleClaim(string code, string title)
    {
        return new LenientMeritCycleClaim(code, title, false, "", 0, 0.0, LenientMeritCycleClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientMeritCycleClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientMeritCycleClaimStatus next)
    {
        switch (Status)
        {
            case LenientMeritCycleClaimStatus.Open:
                switch (next)
                {
                    case LenientMeritCycleClaimStatus.Reopened:
                        return true;
                }
                return false;
            case LenientMeritCycleClaimStatus.InReview:
                switch (next)
                {
                    case LenientMeritCycleClaimStatus.Resolved:
                        return true;
                }
                return false;
            case LenientMeritCycleClaimStatus.Resolved:
                return next == LenientMeritCycleClaimStatus.Reopened || next == LenientMeritCycleClaimStatus.Archived;
            case LenientMeritCycleClaimStatus.Reopened:
                switch (next)
                {
                    case LenientMeritCycleClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientMeritCycleClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient merit cycle claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Rate - (double)Retries, 2);
    }

    public LenientMeritCycleClaim Copy()
    {
        return new LenientMeritCycleClaim(Code, Title, Pinned, Channel, Retries, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
