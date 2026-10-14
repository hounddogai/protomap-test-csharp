namespace Acme.Workforce.Talent.Nomination.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict nomination score is in its life cycle.</summary>
public enum StrictNominationScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a strict nomination score and its current status.</summary>
public sealed class StrictNominationScore(string code, string name, bool pinned, string reviewer, double budget, double discount, int durationMinutes, double maximum, StrictNominationScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public string Reviewer { get; set; } = reviewer;
    public double Budget { get; set; } = budget;
    public double Discount { get; set; } = discount;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Maximum { get; set; } = maximum;
    public StrictNominationScoreStatus Status { get; set; } = status;

    /// <summary>Starts a strict nomination score in status created.</summary>
    public static StrictNominationScore NewStrictNominationScore(string code, string name)
    {
        return new StrictNominationScore(
            code,
            name,
            false,
            "",
            0.0,
            0.0,
            0,
            0.0,
            StrictNominationScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictNominationScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictNominationScoreStatus next)
    {
        switch (Status)
        {
            case StrictNominationScoreStatus.Created:
                switch (next)
                {
                    case StrictNominationScoreStatus.Validated:
                        return true;
                }
                return false;
            case StrictNominationScoreStatus.Validated:
                return next == StrictNominationScoreStatus.Published || next == StrictNominationScoreStatus.Retired;
            case StrictNominationScoreStatus.Published:
                switch (next)
                {
                    case StrictNominationScoreStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictNominationScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict nomination score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict nomination score"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)DurationMinutes * Maximum, 2);
    }

    public StrictNominationScore Copy()
    {
        return new StrictNominationScore(
            Code,
            Name,
            Pinned,
            Reviewer,
            Budget,
            Discount,
            DurationMinutes,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
