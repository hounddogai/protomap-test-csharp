namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal feedback claim is in its life cycle.</summary>
public enum SeasonalFeedbackClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A seasonal feedback claim as the feedback team keeps it.</summary>
public sealed class SeasonalFeedbackClaim(string code, string title, int endsAt, double maximum, string reviewer, int limit, double rate, double cost, string salaryBand, int quantity, SeasonalFeedbackClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public double Maximum { get; set; } = maximum;
    public string Reviewer { get; set; } = reviewer;
    public int Limit { get; set; } = limit;
    public double Rate { get; set; } = rate;
    public double Cost { get; set; } = cost;
    public string SalaryBand { get; set; } = salaryBand;
    public int Quantity { get; set; } = quantity;
    public SeasonalFeedbackClaimStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal feedback claim in status proposed.</summary>
    public static SeasonalFeedbackClaim NewSeasonalFeedbackClaim(string code, string title)
    {
        return new SeasonalFeedbackClaim(
            code,
            title,
            0,
            0.0,
            "",
            0,
            0.0,
            0.0,
            "",
            0,
            SeasonalFeedbackClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFeedbackClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFeedbackClaimStatus next)
    {
        switch (Status)
        {
            case SeasonalFeedbackClaimStatus.Proposed:
                switch (next)
                {
                    case SeasonalFeedbackClaimStatus.Rejected:
                    case SeasonalFeedbackClaimStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalFeedbackClaimStatus.Approved:
                switch (next)
                {
                    case SeasonalFeedbackClaimStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalFeedbackClaimStatus.Rejected:
                switch (next)
                {
                    case SeasonalFeedbackClaimStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalFeedbackClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal feedback claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Cost - (double)EndsAt, 2);
    }

    public SeasonalFeedbackClaim Copy()
    {
        return new SeasonalFeedbackClaim(
            Code,
            Title,
            EndsAt,
            Maximum,
            Reviewer,
            Limit,
            Rate,
            Cost,
            SalaryBand,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
