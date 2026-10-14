namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal cost model preference is in its life cycle.</summary>
public enum SeasonalCostModelPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal cost model preference. Its code is unique within a tenant.</summary>
public sealed class SeasonalCostModelPreference(string code, string name, double floorValue, string country, int quantity, string team, double rate, SeasonalCostModelPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Country { get; set; } = country;
    public int Quantity { get; set; } = quantity;
    public string Team { get; set; } = team;
    public double Rate { get; set; } = rate;
    public SeasonalCostModelPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal cost model preference in status planned.</summary>
    public static SeasonalCostModelPreference NewSeasonalCostModelPreference(string code, string name)
    {
        return new SeasonalCostModelPreference(
            code,
            name,
            0.0,
            "",
            0,
            "",
            0.0,
            SeasonalCostModelPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalCostModelPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalCostModelPreferenceStatus next)
    {
        switch (Status)
        {
            case SeasonalCostModelPreferenceStatus.Planned:
                return next == SeasonalCostModelPreferenceStatus.Cancelled;
            case SeasonalCostModelPreferenceStatus.Scheduled:
                return next == SeasonalCostModelPreferenceStatus.Completed;
            case SeasonalCostModelPreferenceStatus.InProgress:
                switch (next)
                {
                    case SeasonalCostModelPreferenceStatus.Completed:
                    case SeasonalCostModelPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalCostModelPreferenceStatus.Completed:
                return next == SeasonalCostModelPreferenceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SeasonalCostModelPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal cost model preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(FloorValue * (double)Quantity, 2);
    }

    public SeasonalCostModelPreference Copy()
    {
        return new SeasonalCostModelPreference(Code, Name, FloorValue, Country, Quantity, Team, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
