namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive benchmark notice is in its life cycle.</summary>
public enum AdaptiveBenchmarkNoticeStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adaptive benchmark notice and its current status.</summary>
public sealed class AdaptiveBenchmarkNotice(string code, string label, string currency, double ceiling, string costCenter, double score, string managerId, double minimum, double discount, double hours, int retries, AdaptiveBenchmarkNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public double Ceiling { get; set; } = ceiling;
    public string CostCenter { get; set; } = costCenter;
    public double Score { get; set; } = score;
    public string ManagerId { get; set; } = managerId;
    public double Minimum { get; set; } = minimum;
    public double Discount { get; set; } = discount;
    public double Hours { get; set; } = hours;
    public int Retries { get; set; } = retries;
    public AdaptiveBenchmarkNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive benchmark notice in status idle.</summary>
    public static AdaptiveBenchmarkNotice NewAdaptiveBenchmarkNotice(string code, string label)
    {
        return new AdaptiveBenchmarkNotice(
            code,
            label,
            "",
            0.0,
            "",
            0.0,
            "",
            0.0,
            0.0,
            0.0,
            0,
            AdaptiveBenchmarkNoticeStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBenchmarkNoticeStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBenchmarkNoticeStatus next)
    {
        switch (Status)
        {
            case AdaptiveBenchmarkNoticeStatus.Idle:
                switch (next)
                {
                    case AdaptiveBenchmarkNoticeStatus.Warming:
                    case AdaptiveBenchmarkNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveBenchmarkNoticeStatus.Warming:
                switch (next)
                {
                    case AdaptiveBenchmarkNoticeStatus.Ready:
                    case AdaptiveBenchmarkNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveBenchmarkNoticeStatus.Ready:
                switch (next)
                {
                    case AdaptiveBenchmarkNoticeStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveBenchmarkNoticeStatus.Draining:
                switch (next)
                {
                    case AdaptiveBenchmarkNoticeStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveBenchmarkNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive benchmark notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetValue()
    {
        return Math.Round(Hours + (double)Retries, 2);
    }

    public AdaptiveBenchmarkNotice Copy()
    {
        return new AdaptiveBenchmarkNotice(
            Code,
            Label,
            Currency,
            Ceiling,
            CostCenter,
            Score,
            ManagerId,
            Minimum,
            Discount,
            Hours,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
