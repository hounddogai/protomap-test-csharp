namespace Acme.Workforce.Orgdesign.Scenario.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal scenario tier is in its life cycle.</summary>
public enum SeasonalScenarioTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A seasonal scenario tier. Its code is unique within a tenant.</summary>
public sealed class SeasonalScenarioTier(string code, string name, double amount, int tenureMonths, string description, SeasonalScenarioTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Description { get; set; } = description;
    public SeasonalScenarioTierStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal scenario tier in status created.</summary>
    public static SeasonalScenarioTier NewSeasonalScenarioTier(string code, string name)
    {
        return new SeasonalScenarioTier(code, name, 0.0, 0, "", SeasonalScenarioTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalScenarioTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalScenarioTierStatus next)
    {
        switch (Status)
        {
            case SeasonalScenarioTierStatus.Created:
                return next == SeasonalScenarioTierStatus.Published;
            case SeasonalScenarioTierStatus.Validated:
                switch (next)
                {
                    case SeasonalScenarioTierStatus.Published:
                        return true;
                }
                return false;
            case SeasonalScenarioTierStatus.Published:
                return next == SeasonalScenarioTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SeasonalScenarioTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal scenario tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)TenureMonths * Amount, 2);
    }

    public SeasonalScenarioTier Copy()
    {
        return new SeasonalScenarioTier(Code, Name, Amount, TenureMonths, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
