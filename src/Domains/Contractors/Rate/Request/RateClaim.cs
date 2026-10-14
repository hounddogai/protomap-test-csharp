namespace Acme.Workforce.Contractors.Rate.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate claim is in its life cycle.</summary>
public enum RateClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One rate claim, with what the planners need to act on it.</summary>
public sealed class RateClaim(string code, string name, double margin, int startsAt, string jobCode, int priority, RateClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public int StartsAt { get; set; } = startsAt;
    public string JobCode { get; set; } = jobCode;
    public int Priority { get; set; } = priority;
    public RateClaimStatus Status { get; set; } = status;

    /// <summary>Starts a rate claim in status draft.</summary>
    public static RateClaim NewRateClaim(string code, string name)
    {
        return new RateClaim(code, name, 0.0, 0, "", 0, RateClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateClaimStatus next)
    {
        switch (Status)
        {
            case RateClaimStatus.Draft:
                return next == RateClaimStatus.Active || next == RateClaimStatus.Suspended;
            case RateClaimStatus.Pending:
                return next == RateClaimStatus.Active || next == RateClaimStatus.Closed;
            case RateClaimStatus.Active:
                switch (next)
                {
                    case RateClaimStatus.Suspended:
                        return true;
                }
                return false;
            case RateClaimStatus.Suspended:
                switch (next)
                {
                    case RateClaimStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Priority + (double)StartsAt, 2);
    }

    public RateClaim Copy()
    {
        return new RateClaim(Code, Name, Margin, StartsAt, JobCode, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
