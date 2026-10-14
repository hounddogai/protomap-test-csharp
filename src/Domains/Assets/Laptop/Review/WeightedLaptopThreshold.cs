namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted laptop threshold is in its life cycle.</summary>
public enum WeightedLaptopThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a weighted laptop threshold and its current status.</summary>
public sealed class WeightedLaptopThreshold(string code, string title, int startsAt, double floorValue, string label, int windowDays, int tenureMonths, string skill, double weight, string team, double margin, WeightedLaptopThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public double FloorValue { get; set; } = floorValue;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Skill { get; set; } = skill;
    public double Weight { get; set; } = weight;
    public string Team { get; set; } = team;
    public double Margin { get; set; } = margin;
    public WeightedLaptopThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a weighted laptop threshold in status requested.</summary>
    public static WeightedLaptopThreshold NewWeightedLaptopThreshold(string code, string title)
    {
        return new WeightedLaptopThreshold(
            code,
            title,
            0,
            0.0,
            "",
            0,
            0,
            "",
            0.0,
            "",
            0.0,
            WeightedLaptopThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedLaptopThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedLaptopThresholdStatus next)
    {
        switch (Status)
        {
            case WeightedLaptopThresholdStatus.Requested:
                return next == WeightedLaptopThresholdStatus.Revoked;
            case WeightedLaptopThresholdStatus.Granted:
                switch (next)
                {
                    case WeightedLaptopThresholdStatus.Expired:
                        return true;
                }
                return false;
            case WeightedLaptopThresholdStatus.Revoked:
                return next == WeightedLaptopThresholdStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WeightedLaptopThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted laptop threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)WindowDays - FloorValue, 2);
    }

    public WeightedLaptopThreshold Copy()
    {
        return new WeightedLaptopThreshold(
            Code,
            Title,
            StartsAt,
            FloorValue,
            Label,
            WindowDays,
            TenureMonths,
            Skill,
            Weight,
            Team,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
