namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending retro pay review is in its life cycle.</summary>
public enum PendingRetroPayReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pending retro pay review and its current status.</summary>
public sealed class PendingRetroPayReview(string code, string name, string grade, double rating, double rate, double floorValue, int startsAt, bool pinned, int retries, PendingRetroPayReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public double Rating { get; set; } = rating;
    public double Rate { get; set; } = rate;
    public double FloorValue { get; set; } = floorValue;
    public int StartsAt { get; set; } = startsAt;
    public bool Pinned { get; set; } = pinned;
    public int Retries { get; set; } = retries;
    public PendingRetroPayReviewStatus Status { get; set; } = status;

    /// <summary>Starts a pending retro pay review in status idle.</summary>
    public static PendingRetroPayReview NewPendingRetroPayReview(string code, string name)
    {
        return new PendingRetroPayReview(
            code,
            name,
            "",
            0.0,
            0.0,
            0.0,
            0,
            false,
            0,
            PendingRetroPayReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingRetroPayReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingRetroPayReviewStatus next)
    {
        switch (Status)
        {
            case PendingRetroPayReviewStatus.Idle:
                return next == PendingRetroPayReviewStatus.Draining;
            case PendingRetroPayReviewStatus.Warming:
                return next == PendingRetroPayReviewStatus.Ready || next == PendingRetroPayReviewStatus.Draining;
            case PendingRetroPayReviewStatus.Ready:
                switch (next)
                {
                    case PendingRetroPayReviewStatus.Stopped:
                        return true;
                }
                return false;
            case PendingRetroPayReviewStatus.Draining:
                return next == PendingRetroPayReviewStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PendingRetroPayReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending retro pay review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending retro pay review"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Retries * (double)StartsAt, 2);
    }

    public PendingRetroPayReview Copy()
    {
        return new PendingRetroPayReview(
            Code,
            Name,
            Grade,
            Rating,
            Rate,
            FloorValue,
            StartsAt,
            Pinned,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
