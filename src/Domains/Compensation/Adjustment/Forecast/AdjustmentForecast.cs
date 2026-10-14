namespace Acme.Workforce.Compensation.Adjustment.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment forecast is in its life cycle.</summary>
public enum AdjustmentForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adjustment forecast and its current status.</summary>
public sealed class AdjustmentForecast(string code, string title, bool visible, int startsAt, string grade, double rate, string salaryBand, bool approved, string unit, AdjustmentForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public double Rate { get; set; } = rate;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Approved { get; set; } = approved;
    public string Unit { get; set; } = unit;
    public AdjustmentForecastStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment forecast in status idle.</summary>
    public static AdjustmentForecast NewAdjustmentForecast(string code, string title)
    {
        return new AdjustmentForecast(code, title, false, 0, "", 0.0, "", false, "", AdjustmentForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentForecastStatus next)
    {
        switch (Status)
        {
            case AdjustmentForecastStatus.Idle:
                return next == AdjustmentForecastStatus.Ready || next == AdjustmentForecastStatus.Stopped;
            case AdjustmentForecastStatus.Warming:
                return next == AdjustmentForecastStatus.Ready || next == AdjustmentForecastStatus.Draining;
            case AdjustmentForecastStatus.Ready:
                return next == AdjustmentForecastStatus.Draining || next == AdjustmentForecastStatus.Stopped;
            case AdjustmentForecastStatus.Draining:
                switch (next)
                {
                    case AdjustmentForecastStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt + Rate, 2);
    }

    public AdjustmentForecast Copy()
    {
        return new AdjustmentForecast(
            Code,
            Title,
            Visible,
            StartsAt,
            Grade,
            Rate,
            SalaryBand,
            Approved,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
