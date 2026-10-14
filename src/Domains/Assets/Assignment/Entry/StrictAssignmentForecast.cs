namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict assignment forecast is in its life cycle.</summary>
public enum StrictAssignmentForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A strict assignment forecast. Its code is unique within a tenant.</summary>
public sealed class StrictAssignmentForecast(string code, string name, string badge, string costCenter, string location, string contactEmail, int batchSize, int updatedAt, int version, StrictAssignmentForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public string CostCenter { get; set; } = costCenter;
    public string Location { get; set; } = location;
    public string ContactEmail { get; set; } = contactEmail;
    public int BatchSize { get; set; } = batchSize;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Version { get; set; } = version;
    public StrictAssignmentForecastStatus Status { get; set; } = status;

    /// <summary>Starts a strict assignment forecast in status draft.</summary>
    public static StrictAssignmentForecast NewStrictAssignmentForecast(string code, string name)
    {
        return new StrictAssignmentForecast(
            code,
            name,
            "",
            "",
            "",
            "",
            0,
            0,
            0,
            StrictAssignmentForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAssignmentForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAssignmentForecastStatus next)
    {
        switch (Status)
        {
            case StrictAssignmentForecastStatus.Draft:
                switch (next)
                {
                    case StrictAssignmentForecastStatus.Active:
                        return true;
                }
                return false;
            case StrictAssignmentForecastStatus.Pending:
                switch (next)
                {
                    case StrictAssignmentForecastStatus.Active:
                    case StrictAssignmentForecastStatus.Closed:
                        return true;
                }
                return false;
            case StrictAssignmentForecastStatus.Active:
                return next == StrictAssignmentForecastStatus.Suspended || next == StrictAssignmentForecastStatus.Closed;
            case StrictAssignmentForecastStatus.Suspended:
                switch (next)
                {
                    case StrictAssignmentForecastStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictAssignmentForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict assignment forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Version * (double)BatchSize, 2);
    }

    public StrictAssignmentForecast Copy()
    {
        return new StrictAssignmentForecast(
            Code,
            Name,
            Badge,
            CostCenter,
            Location,
            ContactEmail,
            BatchSize,
            UpdatedAt,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
