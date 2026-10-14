namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop estimate is in its life cycle.</summary>
public enum LaptopEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A laptop estimate as the laptop team keeps it.</summary>
public sealed class LaptopEstimate(string code, string name, double floorValue, string segment, double hours, string category, int expiresAt, int retries, LaptopEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Segment { get; set; } = segment;
    public double Hours { get; set; } = hours;
    public string Category { get; set; } = category;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Retries { get; set; } = retries;
    public LaptopEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a laptop estimate in status draft.</summary>
    public static LaptopEstimate NewLaptopEstimate(string code, string name)
    {
        return new LaptopEstimate(code, name, 0.0, "", 0.0, "", 0, 0, LaptopEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopEstimateStatus next)
    {
        switch (Status)
        {
            case LaptopEstimateStatus.Draft:
                return next == LaptopEstimateStatus.Active || next == LaptopEstimateStatus.Closed;
            case LaptopEstimateStatus.Pending:
                return next == LaptopEstimateStatus.Suspended;
            case LaptopEstimateStatus.Active:
                switch (next)
                {
                    case LaptopEstimateStatus.Suspended:
                        return true;
                }
                return false;
            case LaptopEstimateStatus.Suspended:
                switch (next)
                {
                    case LaptopEstimateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Retries + (double)ExpiresAt, 2);
    }

    public LaptopEstimate Copy()
    {
        return new LaptopEstimate(Code, Name, FloorValue, Segment, Hours, Category, ExpiresAt, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
