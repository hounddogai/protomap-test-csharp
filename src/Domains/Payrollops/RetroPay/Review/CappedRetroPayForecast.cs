namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped retro pay forecast is in its life cycle.</summary>
public enum CappedRetroPayForecastStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One capped retro pay forecast, with what the planners need to act on it.</summary>
public sealed class CappedRetroPayForecast(string code, string name, int capacity, List<string> tags, int expiresAt, bool visible, int leaveDays, CappedRetroPayForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public List<string> Tags { get; set; } = tags;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Visible { get; set; } = visible;
    public int LeaveDays { get; set; } = leaveDays;
    public CappedRetroPayForecastStatus Status { get; set; } = status;

    /// <summary>Starts a capped retro pay forecast in status new.</summary>
    public static CappedRetroPayForecast NewCappedRetroPayForecast(string code, string name)
    {
        return new CappedRetroPayForecast(
            code,
            name,
            0,
            new List<string>(),
            0,
            false,
            0,
            CappedRetroPayForecastStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRetroPayForecastStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRetroPayForecastStatus next)
    {
        switch (Status)
        {
            case CappedRetroPayForecastStatus.New:
                switch (next)
                {
                    case CappedRetroPayForecastStatus.Done:
                    case CappedRetroPayForecastStatus.Failed:
                        return true;
                }
                return false;
            case CappedRetroPayForecastStatus.Queued:
                return next == CappedRetroPayForecastStatus.Failed;
            case CappedRetroPayForecastStatus.Running:
                return next == CappedRetroPayForecastStatus.Done || next == CappedRetroPayForecastStatus.Failed;
            case CappedRetroPayForecastStatus.Done:
                return next == CappedRetroPayForecastStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedRetroPayForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped retro pay forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"capped retro pay forecast"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)LeaveDays + (double)Capacity, 2);
    }

    public CappedRetroPayForecast Copy()
    {
        return new CappedRetroPayForecast(Code, Name, Capacity, Tags, ExpiresAt, Visible, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
