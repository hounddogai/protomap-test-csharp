namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary scenario tier is in its life cycle.</summary>
public enum PrimaryScenarioTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A primary scenario tier as the scenario team keeps it.</summary>
public sealed class PrimaryScenarioTier(string code, string title, double amount, string costCenter, int startsAt, int windowDays, double cost, bool approved, PrimaryScenarioTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string CostCenter { get; set; } = costCenter;
    public int StartsAt { get; set; } = startsAt;
    public int WindowDays { get; set; } = windowDays;
    public double Cost { get; set; } = cost;
    public bool Approved { get; set; } = approved;
    public PrimaryScenarioTierStatus Status { get; set; } = status;

    /// <summary>Starts a primary scenario tier in status proposed.</summary>
    public static PrimaryScenarioTier NewPrimaryScenarioTier(string code, string title)
    {
        return new PrimaryScenarioTier(code, title, 0.0, "", 0, 0, 0.0, false, PrimaryScenarioTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryScenarioTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryScenarioTierStatus next)
    {
        switch (Status)
        {
            case PrimaryScenarioTierStatus.Proposed:
                switch (next)
                {
                    case PrimaryScenarioTierStatus.Approved:
                        return true;
                }
                return false;
            case PrimaryScenarioTierStatus.Approved:
                switch (next)
                {
                    case PrimaryScenarioTierStatus.Withdrawn:
                        return true;
                }
                return false;
            case PrimaryScenarioTierStatus.Rejected:
                switch (next)
                {
                    case PrimaryScenarioTierStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryScenarioTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary scenario tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary scenario tier"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Cost + (double)WindowDays, 2);
    }

    public PrimaryScenarioTier Copy()
    {
        return new PrimaryScenarioTier(
            Code,
            Title,
            Amount,
            CostCenter,
            StartsAt,
            WindowDays,
            Cost,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
