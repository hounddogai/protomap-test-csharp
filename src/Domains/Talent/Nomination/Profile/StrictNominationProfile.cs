namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict nomination profile is in its life cycle.</summary>
public enum StrictNominationProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A strict nomination profile. Its code is unique within a tenant.</summary>
public sealed class StrictNominationProfile(string code, string name, int leaveDays, double rating, string unit, string channel, string region, string country, string reviewer, StrictNominationProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rating { get; set; } = rating;
    public string Unit { get; set; } = unit;
    public string Channel { get; set; } = channel;
    public string Region { get; set; } = region;
    public string Country { get; set; } = country;
    public string Reviewer { get; set; } = reviewer;
    public StrictNominationProfileStatus Status { get; set; } = status;

    /// <summary>Starts a strict nomination profile in status created.</summary>
    public static StrictNominationProfile NewStrictNominationProfile(string code, string name)
    {
        return new StrictNominationProfile(
            code,
            name,
            0,
            0.0,
            "",
            "",
            "",
            "",
            "",
            StrictNominationProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictNominationProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictNominationProfileStatus next)
    {
        switch (Status)
        {
            case StrictNominationProfileStatus.Created:
                switch (next)
                {
                    case StrictNominationProfileStatus.Validated:
                        return true;
                }
                return false;
            case StrictNominationProfileStatus.Validated:
                return next == StrictNominationProfileStatus.Published || next == StrictNominationProfileStatus.Retired;
            case StrictNominationProfileStatus.Published:
                switch (next)
                {
                    case StrictNominationProfileStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictNominationProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict nomination profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict nomination profile"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)LeaveDays + Rating, 2);
    }

    public StrictNominationProfile Copy()
    {
        return new StrictNominationProfile(
            Code,
            Name,
            LeaveDays,
            Rating,
            Unit,
            Channel,
            Region,
            Country,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
