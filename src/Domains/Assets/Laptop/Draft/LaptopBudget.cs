namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop budget is in its life cycle.</summary>
public enum LaptopBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A laptop budget as the laptop team keeps it.</summary>
public sealed class LaptopBudget(string code, string name, string unit, int version, int endsAt, LaptopBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public int Version { get; set; } = version;
    public int EndsAt { get; set; } = endsAt;
    public LaptopBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a laptop budget in status requested.</summary>
    public static LaptopBudget NewLaptopBudget(string code, string name)
    {
        return new LaptopBudget(code, name, "", 0, 0, LaptopBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopBudgetStatus next)
    {
        switch (Status)
        {
            case LaptopBudgetStatus.Requested:
                return next == LaptopBudgetStatus.Revoked;
            case LaptopBudgetStatus.Granted:
                switch (next)
                {
                    case LaptopBudgetStatus.Expired:
                        return true;
                }
                return false;
            case LaptopBudgetStatus.Revoked:
                switch (next)
                {
                    case LaptopBudgetStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"laptop budget"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Version * (double)EndsAt, 2);
    }

    public LaptopBudget Copy()
    {
        return new LaptopBudget(Code, Name, Unit, Version, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
