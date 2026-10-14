namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient laptop metric is in its life cycle.</summary>
public enum LenientLaptopMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One lenient laptop metric, with what the planners need to act on it.</summary>
public sealed class LenientLaptopMetric(string code, string name, double weight, string region, int endsAt, string employeeId, int limit, bool approved, string description, string badge, string title, LenientLaptopMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Region { get; set; } = region;
    public int EndsAt { get; set; } = endsAt;
    public string EmployeeId { get; set; } = employeeId;
    public int Limit { get; set; } = limit;
    public bool Approved { get; set; } = approved;
    public string Description { get; set; } = description;
    public string Badge { get; set; } = badge;
    public string Title { get; set; } = title;
    public LenientLaptopMetricStatus Status { get; set; } = status;

    /// <summary>Starts a lenient laptop metric in status draft.</summary>
    public static LenientLaptopMetric NewLenientLaptopMetric(string code, string name)
    {
        return new LenientLaptopMetric(
            code,
            name,
            0.0,
            "",
            0,
            "",
            0,
            false,
            "",
            "",
            "",
            LenientLaptopMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLaptopMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLaptopMetricStatus next)
    {
        switch (Status)
        {
            case LenientLaptopMetricStatus.Draft:
                switch (next)
                {
                    case LenientLaptopMetricStatus.Active:
                    case LenientLaptopMetricStatus.Closed:
                        return true;
                }
                return false;
            case LenientLaptopMetricStatus.Pending:
                return next == LenientLaptopMetricStatus.Suspended;
            case LenientLaptopMetricStatus.Active:
                switch (next)
                {
                    case LenientLaptopMetricStatus.Suspended:
                    case LenientLaptopMetricStatus.Closed:
                        return true;
                }
                return false;
            case LenientLaptopMetricStatus.Suspended:
                return next == LenientLaptopMetricStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientLaptopMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient laptop metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"lenient laptop metric"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Limit - Weight, 2);
    }

    public LenientLaptopMetric Copy()
    {
        return new LenientLaptopMetric(
            Code,
            Name,
            Weight,
            Region,
            EndsAt,
            EmployeeId,
            Limit,
            Approved,
            Description,
            Badge,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
