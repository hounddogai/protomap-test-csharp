namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted nomination variance is in its life cycle.</summary>
public enum WeightedNominationVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a weighted nomination variance and its current status.</summary>
public sealed class WeightedNominationVariance(string code, string title, double amount, string timezone, int capacity, int leaveDays, string contactEmail, string team, int createdAt, int sequence, double rating, WeightedNominationVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string Timezone { get; set; } = timezone;
    public int Capacity { get; set; } = capacity;
    public int LeaveDays { get; set; } = leaveDays;
    public string ContactEmail { get; set; } = contactEmail;
    public string Team { get; set; } = team;
    public int CreatedAt { get; set; } = createdAt;
    public int Sequence { get; set; } = sequence;
    public double Rating { get; set; } = rating;
    public WeightedNominationVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a weighted nomination variance in status open.</summary>
    public static WeightedNominationVariance NewWeightedNominationVariance(string code, string title)
    {
        return new WeightedNominationVariance(
            code,
            title,
            0.0,
            "",
            0,
            0,
            "",
            "",
            0,
            0,
            0.0,
            WeightedNominationVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedNominationVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedNominationVarianceStatus next)
    {
        switch (Status)
        {
            case WeightedNominationVarianceStatus.Open:
                switch (next)
                {
                    case WeightedNominationVarianceStatus.InReview:
                    case WeightedNominationVarianceStatus.Archived:
                        return true;
                }
                return false;
            case WeightedNominationVarianceStatus.InReview:
                return next == WeightedNominationVarianceStatus.Reopened;
            case WeightedNominationVarianceStatus.Resolved:
                switch (next)
                {
                    case WeightedNominationVarianceStatus.Reopened:
                    case WeightedNominationVarianceStatus.Archived:
                        return true;
                }
                return false;
            case WeightedNominationVarianceStatus.Reopened:
                return next == WeightedNominationVarianceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedNominationVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted nomination variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Rating + (double)Capacity, 2);
    }

    public WeightedNominationVariance Copy()
    {
        return new WeightedNominationVariance(
            Code,
            Title,
            Amount,
            Timezone,
            Capacity,
            LeaveDays,
            ContactEmail,
            Team,
            CreatedAt,
            Sequence,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
