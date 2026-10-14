namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling retro pay summary is in its life cycle.</summary>
public enum RollingRetroPaySummaryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling retro pay summary as the retro pay team keeps it.</summary>
public sealed class RollingRetroPaySummary(string code, string name, string currency, double rate, double ratio, string channel, RollingRetroPaySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public string Channel { get; set; } = channel;
    public RollingRetroPaySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a rolling retro pay summary in status planned.</summary>
    public static RollingRetroPaySummary NewRollingRetroPaySummary(string code, string name)
    {
        return new RollingRetroPaySummary(code, name, "", 0.0, 0.0, "", RollingRetroPaySummaryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRetroPaySummaryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRetroPaySummaryStatus next)
    {
        switch (Status)
        {
            case RollingRetroPaySummaryStatus.Planned:
                switch (next)
                {
                    case RollingRetroPaySummaryStatus.Scheduled:
                    case RollingRetroPaySummaryStatus.InProgress:
                        return true;
                }
                return false;
            case RollingRetroPaySummaryStatus.Scheduled:
                return next == RollingRetroPaySummaryStatus.Completed;
            case RollingRetroPaySummaryStatus.InProgress:
                switch (next)
                {
                    case RollingRetroPaySummaryStatus.Completed:
                        return true;
                }
                return false;
            case RollingRetroPaySummaryStatus.Completed:
                return next == RollingRetroPaySummaryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RollingRetroPaySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling retro pay summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling retro pay summary"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Rate + Ratio, 2);
    }

    public RollingRetroPaySummary Copy()
    {
        return new RollingRetroPaySummary(Code, Name, Currency, Rate, Ratio, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
