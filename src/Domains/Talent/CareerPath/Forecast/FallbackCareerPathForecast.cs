namespace Acme.Workforce.Talent.CareerPath.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback career path forecast is in its life cycle.</summary>
public enum FallbackCareerPathForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback career path forecast as the career path team keeps it.</summary>
public sealed class FallbackCareerPathForecast(string code, string label, int sequence, double margin, double fte, int leaveDays, bool approved, FallbackCareerPathForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public double Margin { get; set; } = margin;
    public double Fte { get; set; } = fte;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Approved { get; set; } = approved;
    public FallbackCareerPathForecastStatus Status { get; set; } = status;

    /// <summary>Starts a fallback career path forecast in status created.</summary>
    public static FallbackCareerPathForecast NewFallbackCareerPathForecast(string code, string label)
    {
        return new FallbackCareerPathForecast(
            code,
            label,
            0,
            0.0,
            0.0,
            0,
            false,
            FallbackCareerPathForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCareerPathForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCareerPathForecastStatus next)
    {
        switch (Status)
        {
            case FallbackCareerPathForecastStatus.Created:
                switch (next)
                {
                    case FallbackCareerPathForecastStatus.Validated:
                    case FallbackCareerPathForecastStatus.Published:
                        return true;
                }
                return false;
            case FallbackCareerPathForecastStatus.Validated:
                switch (next)
                {
                    case FallbackCareerPathForecastStatus.Published:
                    case FallbackCareerPathForecastStatus.Retired:
                        return true;
                }
                return false;
            case FallbackCareerPathForecastStatus.Published:
                return next == FallbackCareerPathForecastStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FallbackCareerPathForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback career path forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback career path forecast"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Sequence + (double)LeaveDays, 2);
    }

    public FallbackCareerPathForecast Copy()
    {
        return new FallbackCareerPathForecast(Code, Label, Sequence, Margin, Fte, LeaveDays, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
