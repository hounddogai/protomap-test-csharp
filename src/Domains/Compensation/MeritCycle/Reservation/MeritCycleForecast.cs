namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle forecast is in its life cycle.</summary>
public enum MeritCycleForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a merit cycle forecast and its current status.</summary>
public sealed class MeritCycleForecast(string code, string title, double amount, int capacity, bool active, MeritCycleForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public int Capacity { get; set; } = capacity;
    public bool Active { get; set; } = active;
    public MeritCycleForecastStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle forecast in status proposed.</summary>
    public static MeritCycleForecast NewMeritCycleForecast(string code, string title)
    {
        return new MeritCycleForecast(code, title, 0.0, 0, false, MeritCycleForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleForecastStatus next)
    {
        switch (Status)
        {
            case MeritCycleForecastStatus.Proposed:
                switch (next)
                {
                    case MeritCycleForecastStatus.Approved:
                    case MeritCycleForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case MeritCycleForecastStatus.Approved:
                return next == MeritCycleForecastStatus.Rejected;
            case MeritCycleForecastStatus.Rejected:
                return next == MeritCycleForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(MeritCycleForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"merit cycle forecast"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Amount * (double)Capacity, 2);
    }

    public MeritCycleForecast Copy()
    {
        return new MeritCycleForecast(Code, Title, Amount, Capacity, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
