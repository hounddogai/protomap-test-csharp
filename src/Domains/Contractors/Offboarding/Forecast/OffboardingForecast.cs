namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding forecast is in its life cycle.</summary>
public enum OffboardingForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A offboarding forecast. Its code is unique within a tenant.</summary>
public sealed class OffboardingForecast(string code, string label, int createdAt, int updatedAt, int revision, double margin, string description, string country, string unit, string title, OffboardingForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Revision { get; set; } = revision;
    public double Margin { get; set; } = margin;
    public string Description { get; set; } = description;
    public string Country { get; set; } = country;
    public string Unit { get; set; } = unit;
    public string Title { get; set; } = title;
    public OffboardingForecastStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding forecast in status draft.</summary>
    public static OffboardingForecast NewOffboardingForecast(string code, string label)
    {
        return new OffboardingForecast(code, label, 0, 0, 0, 0.0, "", "", "", "", OffboardingForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingForecastStatus next)
    {
        switch (Status)
        {
            case OffboardingForecastStatus.Draft:
                switch (next)
                {
                    case OffboardingForecastStatus.Closed:
                        return true;
                }
                return false;
            case OffboardingForecastStatus.Pending:
                switch (next)
                {
                    case OffboardingForecastStatus.Closed:
                        return true;
                }
                return false;
            case OffboardingForecastStatus.Active:
                return next == OffboardingForecastStatus.Suspended || next == OffboardingForecastStatus.Closed;
            case OffboardingForecastStatus.Suspended:
                switch (next)
                {
                    case OffboardingForecastStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round(Margin - (double)CreatedAt, 2);
    }

    public OffboardingForecast Copy()
    {
        return new OffboardingForecast(
            Code,
            Label,
            CreatedAt,
            UpdatedAt,
            Revision,
            Margin,
            Description,
            Country,
            Unit,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
