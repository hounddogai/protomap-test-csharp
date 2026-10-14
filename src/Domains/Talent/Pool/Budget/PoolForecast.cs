namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool forecast is in its life cycle.</summary>
public enum PoolForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pool forecast and its current status.</summary>
public sealed class PoolForecast(string code, string name, string category, string badge, string reviewer, int retries, double discount, double weight, PoolForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Badge { get; set; } = badge;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public double Discount { get; set; } = discount;
    public double Weight { get; set; } = weight;
    public PoolForecastStatus Status { get; set; } = status;

    /// <summary>Starts a pool forecast in status created.</summary>
    public static PoolForecast NewPoolForecast(string code, string name)
    {
        return new PoolForecast(code, name, "", "", "", 0, 0.0, 0.0, PoolForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolForecastStatus next)
    {
        switch (Status)
        {
            case PoolForecastStatus.Created:
                return next == PoolForecastStatus.Published || next == PoolForecastStatus.Retired;
            case PoolForecastStatus.Validated:
                switch (next)
                {
                    case PoolForecastStatus.Retired:
                        return true;
                }
                return false;
            case PoolForecastStatus.Published:
                return next == PoolForecastStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PoolForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Retries + Weight, 2);
    }

    public PoolForecast Copy()
    {
        return new PoolForecast(Code, Name, Category, Badge, Reviewer, Retries, Discount, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
