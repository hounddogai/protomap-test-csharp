namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop claim is in its life cycle.</summary>
public enum LaptopClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a laptop claim and its current status.</summary>
public sealed class LaptopClaim(string code, string name, int version, int startsAt, double amount, double rate, string badge, double budget, double floorValue, LaptopClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public int StartsAt { get; set; } = startsAt;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public string Badge { get; set; } = badge;
    public double Budget { get; set; } = budget;
    public double FloorValue { get; set; } = floorValue;
    public LaptopClaimStatus Status { get; set; } = status;

    /// <summary>Starts a laptop claim in status new.</summary>
    public static LaptopClaim NewLaptopClaim(string code, string name)
    {
        return new LaptopClaim(code, name, 0, 0, 0.0, 0.0, "", 0.0, 0.0, LaptopClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopClaimStatus next)
    {
        switch (Status)
        {
            case LaptopClaimStatus.New:
                return next == LaptopClaimStatus.Done || next == LaptopClaimStatus.Failed;
            case LaptopClaimStatus.Queued:
                switch (next)
                {
                    case LaptopClaimStatus.Running:
                    case LaptopClaimStatus.Failed:
                        return true;
                }
                return false;
            case LaptopClaimStatus.Running:
                return next == LaptopClaimStatus.Done;
            case LaptopClaimStatus.Done:
                return next == LaptopClaimStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LaptopClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)StartsAt - (double)Version, 2);
    }

    public LaptopClaim Copy()
    {
        return new LaptopClaim(Code, Name, Version, StartsAt, Amount, Rate, Badge, Budget, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
