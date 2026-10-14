namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted equity grant window is in its life cycle.</summary>
public enum WeightedEquityGrantWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a weighted equity grant window and its current status.</summary>
public sealed class WeightedEquityGrantWindow(string code, string name, int leaveDays, int capacity, string country, int graceDays, string managerId, string badge, double minimum, WeightedEquityGrantWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int Capacity { get; set; } = capacity;
    public string Country { get; set; } = country;
    public int GraceDays { get; set; } = graceDays;
    public string ManagerId { get; set; } = managerId;
    public string Badge { get; set; } = badge;
    public double Minimum { get; set; } = minimum;
    public WeightedEquityGrantWindowStatus Status { get; set; } = status;

    /// <summary>Starts a weighted equity grant window in status planned.</summary>
    public static WeightedEquityGrantWindow NewWeightedEquityGrantWindow(string code, string name)
    {
        return new WeightedEquityGrantWindow(
            code,
            name,
            0,
            0,
            "",
            0,
            "",
            "",
            0.0,
            WeightedEquityGrantWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedEquityGrantWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedEquityGrantWindowStatus next)
    {
        switch (Status)
        {
            case WeightedEquityGrantWindowStatus.Planned:
                switch (next)
                {
                    case WeightedEquityGrantWindowStatus.Completed:
                        return true;
                }
                return false;
            case WeightedEquityGrantWindowStatus.Scheduled:
                switch (next)
                {
                    case WeightedEquityGrantWindowStatus.Completed:
                    case WeightedEquityGrantWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedEquityGrantWindowStatus.InProgress:
                switch (next)
                {
                    case WeightedEquityGrantWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedEquityGrantWindowStatus.Completed:
                switch (next)
                {
                    case WeightedEquityGrantWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedEquityGrantWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted equity grant window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted equity grant window"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Capacity + (double)LeaveDays, 2);
    }

    public WeightedEquityGrantWindow Copy()
    {
        return new WeightedEquityGrantWindow(
            Code,
            Name,
            LeaveDays,
            Capacity,
            Country,
            GraceDays,
            ManagerId,
            Badge,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
