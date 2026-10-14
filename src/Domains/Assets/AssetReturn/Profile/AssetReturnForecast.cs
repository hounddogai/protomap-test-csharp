namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return forecast is in its life cycle.</summary>
public enum AssetReturnForecastStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One asset return forecast, with what the planners need to act on it.</summary>
public sealed class AssetReturnForecast(string code, string name, double cost, int startsAt, string notes, AssetReturnForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public int StartsAt { get; set; } = startsAt;
    public string Notes { get; set; } = notes;
    public AssetReturnForecastStatus Status { get; set; } = status;

    /// <summary>Starts a asset return forecast in status open.</summary>
    public static AssetReturnForecast NewAssetReturnForecast(string code, string name)
    {
        return new AssetReturnForecast(code, name, 0.0, 0, "", AssetReturnForecastStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnForecastStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnForecastStatus next)
    {
        switch (Status)
        {
            case AssetReturnForecastStatus.Open:
                switch (next)
                {
                    case AssetReturnForecastStatus.InReview:
                        return true;
                }
                return false;
            case AssetReturnForecastStatus.InReview:
                return next == AssetReturnForecastStatus.Archived;
            case AssetReturnForecastStatus.Resolved:
                return next == AssetReturnForecastStatus.Reopened || next == AssetReturnForecastStatus.Archived;
            case AssetReturnForecastStatus.Reopened:
                return next == AssetReturnForecastStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AssetReturnForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"asset return forecast"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)StartsAt - Cost, 2);
    }

    public AssetReturnForecast Copy()
    {
        return new AssetReturnForecast(Code, Name, Cost, StartsAt, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
