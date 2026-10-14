namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending license seat forecast is in its life cycle.</summary>
public enum PendingLicenseSeatForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending license seat forecast. Its code is unique within a tenant.</summary>
public sealed class PendingLicenseSeatForecast(string code, string title, string costCenter, double budget, int quantity, int tenureMonths, string skill, int createdAt, int graceDays, PendingLicenseSeatForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public double Budget { get; set; } = budget;
    public int Quantity { get; set; } = quantity;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Skill { get; set; } = skill;
    public int CreatedAt { get; set; } = createdAt;
    public int GraceDays { get; set; } = graceDays;
    public PendingLicenseSeatForecastStatus Status { get; set; } = status;

    /// <summary>Starts a pending license seat forecast in status planned.</summary>
    public static PendingLicenseSeatForecast NewPendingLicenseSeatForecast(string code, string title)
    {
        return new PendingLicenseSeatForecast(
            code,
            title,
            "",
            0.0,
            0,
            0,
            "",
            0,
            0,
            PendingLicenseSeatForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingLicenseSeatForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingLicenseSeatForecastStatus next)
    {
        switch (Status)
        {
            case PendingLicenseSeatForecastStatus.Planned:
                switch (next)
                {
                    case PendingLicenseSeatForecastStatus.Scheduled:
                    case PendingLicenseSeatForecastStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingLicenseSeatForecastStatus.Scheduled:
                switch (next)
                {
                    case PendingLicenseSeatForecastStatus.Completed:
                    case PendingLicenseSeatForecastStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingLicenseSeatForecastStatus.InProgress:
                return next == PendingLicenseSeatForecastStatus.Completed || next == PendingLicenseSeatForecastStatus.Cancelled;
            case PendingLicenseSeatForecastStatus.Completed:
                return next == PendingLicenseSeatForecastStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PendingLicenseSeatForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending license seat forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending license seat forecast"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)TenureMonths + (double)CreatedAt, 2);
    }

    public PendingLicenseSeatForecast Copy()
    {
        return new PendingLicenseSeatForecast(
            Code,
            Title,
            CostCenter,
            Budget,
            Quantity,
            TenureMonths,
            Skill,
            CreatedAt,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
