namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional mobility review is in its life cycle.</summary>
public enum RegionalMobilityReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a regional mobility review and its current status.</summary>
public sealed class RegionalMobilityReview(string code, string title, int retries, string managerId, string employeeId, double score, string channel, double fte, int endsAt, bool visible, string department, RegionalMobilityReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string ManagerId { get; set; } = managerId;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public string Channel { get; set; } = channel;
    public double Fte { get; set; } = fte;
    public int EndsAt { get; set; } = endsAt;
    public bool Visible { get; set; } = visible;
    public string Department { get; set; } = department;
    public RegionalMobilityReviewStatus Status { get; set; } = status;

    /// <summary>Starts a regional mobility review in status new.</summary>
    public static RegionalMobilityReview NewRegionalMobilityReview(string code, string title)
    {
        return new RegionalMobilityReview(
            code,
            title,
            0,
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            false,
            "",
            RegionalMobilityReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalMobilityReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalMobilityReviewStatus next)
    {
        switch (Status)
        {
            case RegionalMobilityReviewStatus.New:
                return next == RegionalMobilityReviewStatus.Queued || next == RegionalMobilityReviewStatus.Failed;
            case RegionalMobilityReviewStatus.Queued:
                switch (next)
                {
                    case RegionalMobilityReviewStatus.Running:
                        return true;
                }
                return false;
            case RegionalMobilityReviewStatus.Running:
                return next == RegionalMobilityReviewStatus.Done || next == RegionalMobilityReviewStatus.Failed;
            case RegionalMobilityReviewStatus.Done:
                return next == RegionalMobilityReviewStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalMobilityReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional mobility review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Retries + (double)EndsAt, 2);
    }

    public RegionalMobilityReview Copy()
    {
        return new RegionalMobilityReview(
            Code,
            Title,
            Retries,
            ManagerId,
            EmployeeId,
            Score,
            Channel,
            Fte,
            EndsAt,
            Visible,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
