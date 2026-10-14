namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched merit cycle review is in its life cycle.</summary>
public enum BatchedMeritCycleReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a batched merit cycle review and its current status.</summary>
public sealed class BatchedMeritCycleReview(string code, string title, string managerId, string region, int tenureMonths, int revision, double rate, double fte, int startsAt, BatchedMeritCycleReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Region { get; set; } = region;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Revision { get; set; } = revision;
    public double Rate { get; set; } = rate;
    public double Fte { get; set; } = fte;
    public int StartsAt { get; set; } = startsAt;
    public BatchedMeritCycleReviewStatus Status { get; set; } = status;

    /// <summary>Starts a batched merit cycle review in status proposed.</summary>
    public static BatchedMeritCycleReview NewBatchedMeritCycleReview(string code, string title)
    {
        return new BatchedMeritCycleReview(
            code,
            title,
            "",
            "",
            0,
            0,
            0.0,
            0.0,
            0,
            BatchedMeritCycleReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedMeritCycleReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedMeritCycleReviewStatus next)
    {
        switch (Status)
        {
            case BatchedMeritCycleReviewStatus.Proposed:
                switch (next)
                {
                    case BatchedMeritCycleReviewStatus.Approved:
                    case BatchedMeritCycleReviewStatus.Rejected:
                        return true;
                }
                return false;
            case BatchedMeritCycleReviewStatus.Approved:
                switch (next)
                {
                    case BatchedMeritCycleReviewStatus.Rejected:
                        return true;
                }
                return false;
            case BatchedMeritCycleReviewStatus.Rejected:
                return next == BatchedMeritCycleReviewStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedMeritCycleReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched merit cycle review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)StartsAt - Rate, 2);
    }

    public BatchedMeritCycleReview Copy()
    {
        return new BatchedMeritCycleReview(
            Code,
            Title,
            ManagerId,
            Region,
            TenureMonths,
            Revision,
            Rate,
            Fte,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
