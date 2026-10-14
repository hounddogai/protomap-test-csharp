namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy nomination rollup is in its life cycle.</summary>
public enum LegacyNominationRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a legacy nomination rollup and its current status.</summary>
public sealed class LegacyNominationRollup(string code, string name, int updatedAt, string checksum, int leaveDays, double rating, string location, LegacyNominationRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Checksum { get; set; } = checksum;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rating { get; set; } = rating;
    public string Location { get; set; } = location;
    public LegacyNominationRollupStatus Status { get; set; } = status;

    /// <summary>Starts a legacy nomination rollup in status draft.</summary>
    public static LegacyNominationRollup NewLegacyNominationRollup(string code, string name)
    {
        return new LegacyNominationRollup(code, name, 0, "", 0, 0.0, "", LegacyNominationRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyNominationRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyNominationRollupStatus next)
    {
        switch (Status)
        {
            case LegacyNominationRollupStatus.Draft:
                return next == LegacyNominationRollupStatus.Active || next == LegacyNominationRollupStatus.Closed;
            case LegacyNominationRollupStatus.Pending:
                switch (next)
                {
                    case LegacyNominationRollupStatus.Active:
                    case LegacyNominationRollupStatus.Closed:
                        return true;
                }
                return false;
            case LegacyNominationRollupStatus.Active:
                return next == LegacyNominationRollupStatus.Suspended;
            case LegacyNominationRollupStatus.Suspended:
                return next == LegacyNominationRollupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyNominationRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy nomination rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)UpdatedAt - (double)LeaveDays, 2);
    }

    public LegacyNominationRollup Copy()
    {
        return new LegacyNominationRollup(Code, Name, UpdatedAt, Checksum, LeaveDays, Rating, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
