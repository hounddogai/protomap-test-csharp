namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal laptop budget is in its life cycle.</summary>
public enum SeasonalLaptopBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A seasonal laptop budget as the laptop team keeps it.</summary>
public sealed class SeasonalLaptopBudget(string code, string name, double weight, double ratio, double hours, double discount, string badge, int startsAt, double threshold, bool visible, SeasonalLaptopBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public double Ratio { get; set; } = ratio;
    public double Hours { get; set; } = hours;
    public double Discount { get; set; } = discount;
    public string Badge { get; set; } = badge;
    public int StartsAt { get; set; } = startsAt;
    public double Threshold { get; set; } = threshold;
    public bool Visible { get; set; } = visible;
    public SeasonalLaptopBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal laptop budget in status new.</summary>
    public static SeasonalLaptopBudget NewSeasonalLaptopBudget(string code, string name)
    {
        return new SeasonalLaptopBudget(
            code,
            name,
            0.0,
            0.0,
            0.0,
            0.0,
            "",
            0,
            0.0,
            false,
            SeasonalLaptopBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalLaptopBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalLaptopBudgetStatus next)
    {
        switch (Status)
        {
            case SeasonalLaptopBudgetStatus.New:
                switch (next)
                {
                    case SeasonalLaptopBudgetStatus.Queued:
                        return true;
                }
                return false;
            case SeasonalLaptopBudgetStatus.Queued:
                return next == SeasonalLaptopBudgetStatus.Running;
            case SeasonalLaptopBudgetStatus.Running:
                switch (next)
                {
                    case SeasonalLaptopBudgetStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalLaptopBudgetStatus.Done:
                switch (next)
                {
                    case SeasonalLaptopBudgetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalLaptopBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal laptop budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Discount * Threshold, 2);
    }

    public SeasonalLaptopBudget Copy()
    {
        return new SeasonalLaptopBudget(
            Code,
            Name,
            Weight,
            Ratio,
            Hours,
            Discount,
            Badge,
            StartsAt,
            Threshold,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
