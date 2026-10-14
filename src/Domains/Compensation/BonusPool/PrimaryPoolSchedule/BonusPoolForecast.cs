namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool forecast is in its life cycle.</summary>
public enum BonusPoolForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A bonus pool forecast as the bonus pool team keeps it.</summary>
public sealed class BonusPoolForecast(string code, string title, int revision, string summary, int sequence, int attempts, double cost, double maximum, double threshold, double discount, bool pinned, BonusPoolForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Summary { get; set; } = summary;
    public int Sequence { get; set; } = sequence;
    public int Attempts { get; set; } = attempts;
    public double Cost { get; set; } = cost;
    public double Maximum { get; set; } = maximum;
    public double Threshold { get; set; } = threshold;
    public double Discount { get; set; } = discount;
    public bool Pinned { get; set; } = pinned;
    public BonusPoolForecastStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool forecast in status idle.</summary>
    public static BonusPoolForecast NewBonusPoolForecast(string code, string title)
    {
        return new BonusPoolForecast(
            code,
            title,
            0,
            "",
            0,
            0,
            0.0,
            0.0,
            0.0,
            0.0,
            false,
            BonusPoolForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolForecastStatus next)
    {
        switch (Status)
        {
            case BonusPoolForecastStatus.Idle:
                return next == BonusPoolForecastStatus.Warming;
            case BonusPoolForecastStatus.Warming:
                switch (next)
                {
                    case BonusPoolForecastStatus.Ready:
                    case BonusPoolForecastStatus.Stopped:
                        return true;
                }
                return false;
            case BonusPoolForecastStatus.Ready:
                switch (next)
                {
                    case BonusPoolForecastStatus.Draining:
                        return true;
                }
                return false;
            case BonusPoolForecastStatus.Draining:
                return next == BonusPoolForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BonusPoolForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"bonus pool forecast"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Revision - Discount, 2);
    }

    public BonusPoolForecast Copy()
    {
        return new BonusPoolForecast(
            Code,
            Title,
            Revision,
            Summary,
            Sequence,
            Attempts,
            Cost,
            Maximum,
            Threshold,
            Discount,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
