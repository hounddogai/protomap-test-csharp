namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional laptop budget is in its life cycle.</summary>
public enum RegionalLaptopBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One regional laptop budget, with what the planners need to act on it.</summary>
public sealed class RegionalLaptopBudget(string code, string name, int sequence, string region, int durationMinutes, int tenureMonths, double weight, double rate, RegionalLaptopBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public string Region { get; set; } = region;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Weight { get; set; } = weight;
    public double Rate { get; set; } = rate;
    public RegionalLaptopBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a regional laptop budget in status new.</summary>
    public static RegionalLaptopBudget NewRegionalLaptopBudget(string code, string name)
    {
        return new RegionalLaptopBudget(code, name, 0, "", 0, 0, 0.0, 0.0, RegionalLaptopBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalLaptopBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalLaptopBudgetStatus next)
    {
        switch (Status)
        {
            case RegionalLaptopBudgetStatus.New:
                return next == RegionalLaptopBudgetStatus.Queued || next == RegionalLaptopBudgetStatus.Running;
            case RegionalLaptopBudgetStatus.Queued:
                switch (next)
                {
                    case RegionalLaptopBudgetStatus.Failed:
                        return true;
                }
                return false;
            case RegionalLaptopBudgetStatus.Running:
                switch (next)
                {
                    case RegionalLaptopBudgetStatus.Failed:
                        return true;
                }
                return false;
            case RegionalLaptopBudgetStatus.Done:
                return next == RegionalLaptopBudgetStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalLaptopBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional laptop budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional laptop budget"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Weight + (double)TenureMonths, 2);
    }

    public RegionalLaptopBudget Copy()
    {
        return new RegionalLaptopBudget(
            Code,
            Name,
            Sequence,
            Region,
            DurationMinutes,
            TenureMonths,
            Weight,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
