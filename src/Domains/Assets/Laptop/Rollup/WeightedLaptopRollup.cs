namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted laptop rollup is in its life cycle.</summary>
public enum WeightedLaptopRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A weighted laptop rollup as the laptop team keeps it.</summary>
public sealed class WeightedLaptopRollup(string code, string name, string description, int leaveDays, int limit, string costCenter, string notes, string jobCode, string location, WeightedLaptopRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int LeaveDays { get; set; } = leaveDays;
    public int Limit { get; set; } = limit;
    public string CostCenter { get; set; } = costCenter;
    public string Notes { get; set; } = notes;
    public string JobCode { get; set; } = jobCode;
    public string Location { get; set; } = location;
    public WeightedLaptopRollupStatus Status { get; set; } = status;

    /// <summary>Starts a weighted laptop rollup in status created.</summary>
    public static WeightedLaptopRollup NewWeightedLaptopRollup(string code, string name)
    {
        return new WeightedLaptopRollup(code, name, "", 0, 0, "", "", "", "", WeightedLaptopRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedLaptopRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedLaptopRollupStatus next)
    {
        switch (Status)
        {
            case WeightedLaptopRollupStatus.Created:
                switch (next)
                {
                    case WeightedLaptopRollupStatus.Validated:
                    case WeightedLaptopRollupStatus.Published:
                        return true;
                }
                return false;
            case WeightedLaptopRollupStatus.Validated:
                switch (next)
                {
                    case WeightedLaptopRollupStatus.Retired:
                        return true;
                }
                return false;
            case WeightedLaptopRollupStatus.Published:
                switch (next)
                {
                    case WeightedLaptopRollupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedLaptopRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted laptop rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted laptop rollup"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Limit + (double)LeaveDays, 2);
    }

    public WeightedLaptopRollup Copy()
    {
        return new WeightedLaptopRollup(
            Code,
            Name,
            Description,
            LeaveDays,
            Limit,
            CostCenter,
            Notes,
            JobCode,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
