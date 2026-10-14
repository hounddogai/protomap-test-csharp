namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path forecast is in its life cycle.</summary>
public enum CareerPathForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A career path forecast. Its code is unique within a tenant.</summary>
public sealed class CareerPathForecast(string code, string name, string badge, double maximum, List<string> tags, string title, string locale, int expiresAt, string externalRef, string channel, double cost, CareerPathForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public double Maximum { get; set; } = maximum;
    public List<string> Tags { get; set; } = tags;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ExternalRef { get; set; } = externalRef;
    public string Channel { get; set; } = channel;
    public double Cost { get; set; } = cost;
    public CareerPathForecastStatus Status { get; set; } = status;

    /// <summary>Starts a career path forecast in status requested.</summary>
    public static CareerPathForecast NewCareerPathForecast(string code, string name)
    {
        return new CareerPathForecast(
            code,
            name,
            "",
            0.0,
            new List<string>(),
            "",
            "",
            0,
            "",
            "",
            0.0,
            CareerPathForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathForecastStatus next)
    {
        switch (Status)
        {
            case CareerPathForecastStatus.Requested:
                switch (next)
                {
                    case CareerPathForecastStatus.Revoked:
                    case CareerPathForecastStatus.Expired:
                        return true;
                }
                return false;
            case CareerPathForecastStatus.Granted:
                switch (next)
                {
                    case CareerPathForecastStatus.Expired:
                        return true;
                }
                return false;
            case CareerPathForecastStatus.Revoked:
                return next == CareerPathForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CareerPathForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double WeightedVolume()
    {
        return Math.Round(Maximum * (double)ExpiresAt, 2);
    }

    public CareerPathForecast Copy()
    {
        return new CareerPathForecast(
            Code,
            Name,
            Badge,
            Maximum,
            Tags,
            Title,
            Locale,
            ExpiresAt,
            ExternalRef,
            Channel,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
