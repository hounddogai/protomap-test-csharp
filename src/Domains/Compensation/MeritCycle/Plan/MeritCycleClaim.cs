namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle claim is in its life cycle.</summary>
public enum MeritCycleClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a merit cycle claim and its current status.</summary>
public sealed class MeritCycleClaim(string code, string title, string jobCode, string source, double rating, string contactEmail, double maximum, double score, bool active, MeritCycleClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public string Source { get; set; } = source;
    public double Rating { get; set; } = rating;
    public string ContactEmail { get; set; } = contactEmail;
    public double Maximum { get; set; } = maximum;
    public double Score { get; set; } = score;
    public bool Active { get; set; } = active;
    public MeritCycleClaimStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle claim in status planned.</summary>
    public static MeritCycleClaim NewMeritCycleClaim(string code, string title)
    {
        return new MeritCycleClaim(code, title, "", "", 0.0, "", 0.0, 0.0, false, MeritCycleClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleClaimStatus next)
    {
        switch (Status)
        {
            case MeritCycleClaimStatus.Planned:
                return next == MeritCycleClaimStatus.Completed;
            case MeritCycleClaimStatus.Scheduled:
                return next == MeritCycleClaimStatus.Cancelled;
            case MeritCycleClaimStatus.InProgress:
                return next == MeritCycleClaimStatus.Completed || next == MeritCycleClaimStatus.Cancelled;
            case MeritCycleClaimStatus.Completed:
                switch (next)
                {
                    case MeritCycleClaimStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"merit cycle claim"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Rating + Maximum, 2);
    }

    public MeritCycleClaim Copy()
    {
        return new MeritCycleClaim(
            Code,
            Title,
            JobCode,
            Source,
            Rating,
            ContactEmail,
            Maximum,
            Score,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
