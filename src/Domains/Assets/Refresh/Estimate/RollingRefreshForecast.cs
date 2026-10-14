namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling refresh forecast is in its life cycle.</summary>
public enum RollingRefreshForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a rolling refresh forecast and its current status.</summary>
public sealed class RollingRefreshForecast(string code, string title, string department, List<string> tags, int limit, int expiresAt, int createdAt, RollingRefreshForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public List<string> Tags { get; set; } = tags;
    public int Limit { get; set; } = limit;
    public int ExpiresAt { get; set; } = expiresAt;
    public int CreatedAt { get; set; } = createdAt;
    public RollingRefreshForecastStatus Status { get; set; } = status;

    /// <summary>Starts a rolling refresh forecast in status idle.</summary>
    public static RollingRefreshForecast NewRollingRefreshForecast(string code, string title)
    {
        return new RollingRefreshForecast(
            code,
            title,
            "",
            new List<string>(),
            0,
            0,
            0,
            RollingRefreshForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRefreshForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRefreshForecastStatus next)
    {
        switch (Status)
        {
            case RollingRefreshForecastStatus.Idle:
                switch (next)
                {
                    case RollingRefreshForecastStatus.Draining:
                        return true;
                }
                return false;
            case RollingRefreshForecastStatus.Warming:
                switch (next)
                {
                    case RollingRefreshForecastStatus.Draining:
                        return true;
                }
                return false;
            case RollingRefreshForecastStatus.Ready:
                switch (next)
                {
                    case RollingRefreshForecastStatus.Draining:
                    case RollingRefreshForecastStatus.Stopped:
                        return true;
                }
                return false;
            case RollingRefreshForecastStatus.Draining:
                switch (next)
                {
                    case RollingRefreshForecastStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingRefreshForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling refresh forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)ExpiresAt + (double)Limit, 2);
    }

    public RollingRefreshForecast Copy()
    {
        return new RollingRefreshForecast(Code, Title, Department, Tags, Limit, ExpiresAt, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
