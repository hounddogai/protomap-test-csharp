namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal nomination cycle is in its life cycle.</summary>
public enum SeasonalNominationCycleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A seasonal nomination cycle as the nomination team keeps it.</summary>
public sealed class SeasonalNominationCycle(string code, string name, string source, double fte, string ownerId, SeasonalNominationCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public double Fte { get; set; } = fte;
    public string OwnerId { get; set; } = ownerId;
    public SeasonalNominationCycleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal nomination cycle in status draft.</summary>
    public static SeasonalNominationCycle NewSeasonalNominationCycle(string code, string name)
    {
        return new SeasonalNominationCycle(code, name, "", 0.0, "", SeasonalNominationCycleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalNominationCycleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalNominationCycleStatus next)
    {
        switch (Status)
        {
            case SeasonalNominationCycleStatus.Draft:
                switch (next)
                {
                    case SeasonalNominationCycleStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalNominationCycleStatus.Pending:
                return next == SeasonalNominationCycleStatus.Suspended || next == SeasonalNominationCycleStatus.Closed;
            case SeasonalNominationCycleStatus.Active:
                return next == SeasonalNominationCycleStatus.Suspended;
            case SeasonalNominationCycleStatus.Suspended:
                return next == SeasonalNominationCycleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalNominationCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal nomination cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal nomination cycle"} {Code}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public SeasonalNominationCycle Copy()
    {
        return new SeasonalNominationCycle(Code, Name, Source, Fte, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
