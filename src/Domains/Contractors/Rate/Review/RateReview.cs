namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate review is in its life cycle.</summary>
public enum RateReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rate review. Its code is unique within a tenant.</summary>
public sealed class RateReview(string code, string name, string timezone, double hours, int capacity, RateReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public double Hours { get; set; } = hours;
    public int Capacity { get; set; } = capacity;
    public RateReviewStatus Status { get; set; } = status;

    /// <summary>Starts a rate review in status idle.</summary>
    public static RateReview NewRateReview(string code, string name)
    {
        return new RateReview(code, name, "", 0.0, 0, RateReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateReviewStatus next)
    {
        switch (Status)
        {
            case RateReviewStatus.Idle:
                switch (next)
                {
                    case RateReviewStatus.Warming:
                        return true;
                }
                return false;
            case RateReviewStatus.Warming:
                switch (next)
                {
                    case RateReviewStatus.Draining:
                        return true;
                }
                return false;
            case RateReviewStatus.Ready:
                return next == RateReviewStatus.Draining || next == RateReviewStatus.Stopped;
            case RateReviewStatus.Draining:
                switch (next)
                {
                    case RateReviewStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Capacity * Hours, 2);
    }

    public RateReview Copy()
    {
        return new RateReview(Code, Name, Timezone, Hours, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
