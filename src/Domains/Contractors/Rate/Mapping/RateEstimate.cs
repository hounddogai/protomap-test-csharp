namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate estimate is in its life cycle.</summary>
public enum RateEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rate estimate. Its code is unique within a tenant.</summary>
public sealed class RateEstimate(string code, string title, double ratio, string channel, int batchSize, string externalRef, string jobCode, double threshold, string costCenter, int priority, int capacity, RateEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Channel { get; set; } = channel;
    public int BatchSize { get; set; } = batchSize;
    public string ExternalRef { get; set; } = externalRef;
    public string JobCode { get; set; } = jobCode;
    public double Threshold { get; set; } = threshold;
    public string CostCenter { get; set; } = costCenter;
    public int Priority { get; set; } = priority;
    public int Capacity { get; set; } = capacity;
    public RateEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a rate estimate in status idle.</summary>
    public static RateEstimate NewRateEstimate(string code, string title)
    {
        return new RateEstimate(code, title, 0.0, "", 0, "", "", 0.0, "", 0, 0, RateEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateEstimateStatus next)
    {
        switch (Status)
        {
            case RateEstimateStatus.Idle:
                switch (next)
                {
                    case RateEstimateStatus.Draining:
                        return true;
                }
                return false;
            case RateEstimateStatus.Warming:
                switch (next)
                {
                    case RateEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case RateEstimateStatus.Ready:
                return next == RateEstimateStatus.Draining || next == RateEstimateStatus.Stopped;
            case RateEstimateStatus.Draining:
                return next == RateEstimateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RateEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetValue()
    {
        return Math.Round(Threshold * (double)BatchSize, 2);
    }

    public RateEstimate Copy()
    {
        return new RateEstimate(
            Code,
            Title,
            Ratio,
            Channel,
            BatchSize,
            ExternalRef,
            JobCode,
            Threshold,
            CostCenter,
            Priority,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
