namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk forecast is in its life cycle.</summary>
public enum DeskForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a desk forecast and its current status.</summary>
public sealed class DeskForecast(string code, string title, int quantity, bool pinned, int tenureMonths, DeskForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public bool Pinned { get; set; } = pinned;
    public int TenureMonths { get; set; } = tenureMonths;
    public DeskForecastStatus Status { get; set; } = status;

    /// <summary>Starts a desk forecast in status planned.</summary>
    public static DeskForecast NewDeskForecast(string code, string title)
    {
        return new DeskForecast(code, title, 0, false, 0, DeskForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskForecastStatus next)
    {
        switch (Status)
        {
            case DeskForecastStatus.Planned:
                return next == DeskForecastStatus.InProgress;
            case DeskForecastStatus.Scheduled:
                return next == DeskForecastStatus.InProgress || next == DeskForecastStatus.Completed;
            case DeskForecastStatus.InProgress:
                return next == DeskForecastStatus.Completed;
            case DeskForecastStatus.Completed:
                switch (next)
                {
                    case DeskForecastStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Quantity * (double)TenureMonths, 2);
    }

    public DeskForecast Copy()
    {
        return new DeskForecast(Code, Title, Quantity, Pinned, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
