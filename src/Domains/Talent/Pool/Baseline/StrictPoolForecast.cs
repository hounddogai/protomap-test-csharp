namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pool forecast is in its life cycle.</summary>
public enum StrictPoolForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A strict pool forecast. Its code is unique within a tenant.</summary>
public sealed class StrictPoolForecast(string code, string label, int updatedAt, int limit, string checksum, StrictPoolForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Limit { get; set; } = limit;
    public string Checksum { get; set; } = checksum;
    public StrictPoolForecastStatus Status { get; set; } = status;

    /// <summary>Starts a strict pool forecast in status idle.</summary>
    public static StrictPoolForecast NewStrictPoolForecast(string code, string label)
    {
        return new StrictPoolForecast(code, label, 0, 0, "", StrictPoolForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPoolForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPoolForecastStatus next)
    {
        switch (Status)
        {
            case StrictPoolForecastStatus.Idle:
                return next == StrictPoolForecastStatus.Warming;
            case StrictPoolForecastStatus.Warming:
                switch (next)
                {
                    case StrictPoolForecastStatus.Draining:
                        return true;
                }
                return false;
            case StrictPoolForecastStatus.Ready:
                switch (next)
                {
                    case StrictPoolForecastStatus.Stopped:
                        return true;
                }
                return false;
            case StrictPoolForecastStatus.Draining:
                return next == StrictPoolForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictPoolForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pool forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"strict pool forecast"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Limit * (double)UpdatedAt, 2);
    }

    public StrictPoolForecast Copy()
    {
        return new StrictPoolForecast(Code, Label, UpdatedAt, Limit, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
