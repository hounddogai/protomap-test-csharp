namespace Acme.Workforce.Assets.Laptop.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted laptop estimate is in its life cycle.</summary>
public enum WeightedLaptopEstimateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One weighted laptop estimate, with what the planners need to act on it.</summary>
public sealed class WeightedLaptopEstimate(string code, string title, string unit, string checksum, string jobCode, string timezone, double weight, string country, double ceiling, WeightedLaptopEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public string Checksum { get; set; } = checksum;
    public string JobCode { get; set; } = jobCode;
    public string Timezone { get; set; } = timezone;
    public double Weight { get; set; } = weight;
    public string Country { get; set; } = country;
    public double Ceiling { get; set; } = ceiling;
    public WeightedLaptopEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a weighted laptop estimate in status new.</summary>
    public static WeightedLaptopEstimate NewWeightedLaptopEstimate(string code, string title)
    {
        return new WeightedLaptopEstimate(
            code,
            title,
            "",
            "",
            "",
            "",
            0.0,
            "",
            0.0,
            WeightedLaptopEstimateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedLaptopEstimateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedLaptopEstimateStatus next)
    {
        switch (Status)
        {
            case WeightedLaptopEstimateStatus.New:
                return next == WeightedLaptopEstimateStatus.Queued || next == WeightedLaptopEstimateStatus.Done;
            case WeightedLaptopEstimateStatus.Queued:
                return next == WeightedLaptopEstimateStatus.Done || next == WeightedLaptopEstimateStatus.Failed;
            case WeightedLaptopEstimateStatus.Running:
                switch (next)
                {
                    case WeightedLaptopEstimateStatus.Done:
                    case WeightedLaptopEstimateStatus.Failed:
                        return true;
                }
                return false;
            case WeightedLaptopEstimateStatus.Done:
                return next == WeightedLaptopEstimateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(WeightedLaptopEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted laptop estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Ceiling - Weight, 2);
    }

    public WeightedLaptopEstimate Copy()
    {
        return new WeightedLaptopEstimate(
            Code,
            Title,
            Unit,
            Checksum,
            JobCode,
            Timezone,
            Weight,
            Country,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
