namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall budget is in its life cycle.</summary>
public enum TownHallBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A town hall budget. Its code is unique within a tenant.</summary>
public sealed class TownHallBudget(string code, string name, double floorValue, int revision, string costCenter, double weight, int startsAt, Dictionary<string, string> attributes, string description, TownHallBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public string CostCenter { get; set; } = costCenter;
    public double Weight { get; set; } = weight;
    public int StartsAt { get; set; } = startsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Description { get; set; } = description;
    public TownHallBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a town hall budget in status idle.</summary>
    public static TownHallBudget NewTownHallBudget(string code, string name)
    {
        return new TownHallBudget(
            code,
            name,
            0.0,
            0,
            "",
            0.0,
            0,
            new Dictionary<string, string>(),
            "",
            TownHallBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallBudgetStatus next)
    {
        switch (Status)
        {
            case TownHallBudgetStatus.Idle:
                return next == TownHallBudgetStatus.Draining;
            case TownHallBudgetStatus.Warming:
                switch (next)
                {
                    case TownHallBudgetStatus.Ready:
                    case TownHallBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case TownHallBudgetStatus.Ready:
                switch (next)
                {
                    case TownHallBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case TownHallBudgetStatus.Draining:
                return next == TownHallBudgetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TownHallBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double AdjustedVolume()
    {
        return Math.Round(FloorValue + (double)StartsAt, 2);
    }

    public TownHallBudget Copy()
    {
        return new TownHallBudget(
            Code,
            Name,
            FloorValue,
            Revision,
            CostCenter,
            Weight,
            StartsAt,
            Attributes,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
