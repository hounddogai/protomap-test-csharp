namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal merit cycle profile is in its life cycle.</summary>
public enum SeasonalMeritCycleProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A seasonal merit cycle profile. Its code is unique within a tenant.</summary>
public sealed class SeasonalMeritCycleProfile(string code, string name, Dictionary<string, string> attributes, string checksum, string currency, string team, int windowDays, SeasonalMeritCycleProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Checksum { get; set; } = checksum;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public SeasonalMeritCycleProfileStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal merit cycle profile in status created.</summary>
    public static SeasonalMeritCycleProfile NewSeasonalMeritCycleProfile(string code, string name)
    {
        return new SeasonalMeritCycleProfile(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0,
            SeasonalMeritCycleProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalMeritCycleProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalMeritCycleProfileStatus next)
    {
        switch (Status)
        {
            case SeasonalMeritCycleProfileStatus.Created:
                switch (next)
                {
                    case SeasonalMeritCycleProfileStatus.Published:
                        return true;
                }
                return false;
            case SeasonalMeritCycleProfileStatus.Validated:
                switch (next)
                {
                    case SeasonalMeritCycleProfileStatus.Published:
                    case SeasonalMeritCycleProfileStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalMeritCycleProfileStatus.Published:
                switch (next)
                {
                    case SeasonalMeritCycleProfileStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalMeritCycleProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal merit cycle profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal merit cycle profile"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public SeasonalMeritCycleProfile Copy()
    {
        return new SeasonalMeritCycleProfile(Code, Name, Attributes, Checksum, Currency, Team, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
