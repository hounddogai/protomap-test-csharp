namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal mobility forecast is in its life cycle.</summary>
public enum SeasonalMobilityForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One seasonal mobility forecast, with what the planners need to act on it.</summary>
public sealed class SeasonalMobilityForecast(string code, string label, double fte, string summary, int priority, double ceiling, string externalRef, string reviewer, string ownerId, double ratio, string employeeId, SeasonalMobilityForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string Summary { get; set; } = summary;
    public int Priority { get; set; } = priority;
    public double Ceiling { get; set; } = ceiling;
    public string ExternalRef { get; set; } = externalRef;
    public string Reviewer { get; set; } = reviewer;
    public string OwnerId { get; set; } = ownerId;
    public double Ratio { get; set; } = ratio;
    public string EmployeeId { get; set; } = employeeId;
    public SeasonalMobilityForecastStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal mobility forecast in status draft.</summary>
    public static SeasonalMobilityForecast NewSeasonalMobilityForecast(string code, string label)
    {
        return new SeasonalMobilityForecast(
            code,
            label,
            0.0,
            "",
            0,
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            SeasonalMobilityForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalMobilityForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalMobilityForecastStatus next)
    {
        switch (Status)
        {
            case SeasonalMobilityForecastStatus.Draft:
                return next == SeasonalMobilityForecastStatus.Active || next == SeasonalMobilityForecastStatus.Closed;
            case SeasonalMobilityForecastStatus.Pending:
                switch (next)
                {
                    case SeasonalMobilityForecastStatus.Active:
                        return true;
                }
                return false;
            case SeasonalMobilityForecastStatus.Active:
                return next == SeasonalMobilityForecastStatus.Closed;
            case SeasonalMobilityForecastStatus.Suspended:
                return next == SeasonalMobilityForecastStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalMobilityForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal mobility forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"seasonal mobility forecast"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Fte * Ceiling, 2);
    }

    public SeasonalMobilityForecast Copy()
    {
        return new SeasonalMobilityForecast(
            Code,
            Label,
            Fte,
            Summary,
            Priority,
            Ceiling,
            ExternalRef,
            Reviewer,
            OwnerId,
            Ratio,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
