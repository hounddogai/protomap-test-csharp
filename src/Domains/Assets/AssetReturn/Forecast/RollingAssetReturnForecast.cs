namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling asset return forecast is in its life cycle.</summary>
public enum RollingAssetReturnForecastStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling asset return forecast as the asset return team keeps it.</summary>
public sealed class RollingAssetReturnForecast(string code, string title, string ownerId, string badge, int endsAt, int quantity, RollingAssetReturnForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Badge { get; set; } = badge;
    public int EndsAt { get; set; } = endsAt;
    public int Quantity { get; set; } = quantity;
    public RollingAssetReturnForecastStatus Status { get; set; } = status;

    /// <summary>Starts a rolling asset return forecast in status new.</summary>
    public static RollingAssetReturnForecast NewRollingAssetReturnForecast(string code, string title)
    {
        return new RollingAssetReturnForecast(code, title, "", "", 0, 0, RollingAssetReturnForecastStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAssetReturnForecastStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAssetReturnForecastStatus next)
    {
        switch (Status)
        {
            case RollingAssetReturnForecastStatus.New:
                return next == RollingAssetReturnForecastStatus.Running || next == RollingAssetReturnForecastStatus.Failed;
            case RollingAssetReturnForecastStatus.Queued:
                return next == RollingAssetReturnForecastStatus.Failed;
            case RollingAssetReturnForecastStatus.Running:
                return next == RollingAssetReturnForecastStatus.Done || next == RollingAssetReturnForecastStatus.Failed;
            case RollingAssetReturnForecastStatus.Done:
                switch (next)
                {
                    case RollingAssetReturnForecastStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingAssetReturnForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling asset return forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Quantity * (double)EndsAt, 2);
    }

    public RollingAssetReturnForecast Copy()
    {
        return new RollingAssetReturnForecast(Code, Title, OwnerId, Badge, EndsAt, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
