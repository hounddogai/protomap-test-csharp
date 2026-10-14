namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling career path forecast is in its life cycle.</summary>
public enum RollingCareerPathForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling career path forecast as the career path team keeps it.</summary>
public sealed class RollingCareerPathForecast(string code, string title, string name, int createdAt, string grade, int leaveDays, int tenureMonths, double budget, RollingCareerPathForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public string Grade { get; set; } = grade;
    public int LeaveDays { get; set; } = leaveDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Budget { get; set; } = budget;
    public RollingCareerPathForecastStatus Status { get; set; } = status;

    /// <summary>Starts a rolling career path forecast in status draft.</summary>
    public static RollingCareerPathForecast NewRollingCareerPathForecast(string code, string title)
    {
        return new RollingCareerPathForecast(
            code,
            title,
            "",
            0,
            "",
            0,
            0,
            0.0,
            RollingCareerPathForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingCareerPathForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingCareerPathForecastStatus next)
    {
        switch (Status)
        {
            case RollingCareerPathForecastStatus.Draft:
                switch (next)
                {
                    case RollingCareerPathForecastStatus.Suspended:
                    case RollingCareerPathForecastStatus.Closed:
                        return true;
                }
                return false;
            case RollingCareerPathForecastStatus.Pending:
                switch (next)
                {
                    case RollingCareerPathForecastStatus.Suspended:
                    case RollingCareerPathForecastStatus.Closed:
                        return true;
                }
                return false;
            case RollingCareerPathForecastStatus.Active:
                switch (next)
                {
                    case RollingCareerPathForecastStatus.Suspended:
                        return true;
                }
                return false;
            case RollingCareerPathForecastStatus.Suspended:
                switch (next)
                {
                    case RollingCareerPathForecastStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingCareerPathForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling career path forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)CreatedAt - (double)LeaveDays, 2);
    }

    public RollingCareerPathForecast Copy()
    {
        return new RollingCareerPathForecast(
            Code,
            Title,
            Name,
            CreatedAt,
            Grade,
            LeaveDays,
            TenureMonths,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
