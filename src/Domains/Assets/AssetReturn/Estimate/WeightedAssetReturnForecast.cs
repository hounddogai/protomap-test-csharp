namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted asset return forecast is in its life cycle.</summary>
public enum WeightedAssetReturnForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A weighted asset return forecast. Its code is unique within a tenant.</summary>
public sealed class WeightedAssetReturnForecast(string code, string name, int capacity, double cost, string country, string unit, bool approved, int expiresAt, int createdAt, WeightedAssetReturnForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Cost { get; set; } = cost;
    public string Country { get; set; } = country;
    public string Unit { get; set; } = unit;
    public bool Approved { get; set; } = approved;
    public int ExpiresAt { get; set; } = expiresAt;
    public int CreatedAt { get; set; } = createdAt;
    public WeightedAssetReturnForecastStatus Status { get; set; } = status;

    /// <summary>Starts a weighted asset return forecast in status idle.</summary>
    public static WeightedAssetReturnForecast NewWeightedAssetReturnForecast(string code, string name)
    {
        return new WeightedAssetReturnForecast(
            code,
            name,
            0,
            0.0,
            "",
            "",
            false,
            0,
            0,
            WeightedAssetReturnForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAssetReturnForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAssetReturnForecastStatus next)
    {
        switch (Status)
        {
            case WeightedAssetReturnForecastStatus.Idle:
                return next == WeightedAssetReturnForecastStatus.Warming || next == WeightedAssetReturnForecastStatus.Draining;
            case WeightedAssetReturnForecastStatus.Warming:
                return next == WeightedAssetReturnForecastStatus.Draining;
            case WeightedAssetReturnForecastStatus.Ready:
                return next == WeightedAssetReturnForecastStatus.Draining || next == WeightedAssetReturnForecastStatus.Stopped;
            case WeightedAssetReturnForecastStatus.Draining:
                switch (next)
                {
                    case WeightedAssetReturnForecastStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedAssetReturnForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted asset return forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted asset return forecast"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Cost + (double)ExpiresAt, 2);
    }

    public WeightedAssetReturnForecast Copy()
    {
        return new WeightedAssetReturnForecast(
            Code,
            Name,
            Capacity,
            Cost,
            Country,
            Unit,
            Approved,
            ExpiresAt,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
