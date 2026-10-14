namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pay band threshold is in its life cycle.</summary>
public enum CappedPayBandThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One capped pay band threshold, with what the planners need to act on it.</summary>
public sealed class CappedPayBandThreshold(string code, string label, int durationMinutes, bool approved, int leaveDays, string costCenter, double discount, CappedPayBandThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Approved { get; set; } = approved;
    public int LeaveDays { get; set; } = leaveDays;
    public string CostCenter { get; set; } = costCenter;
    public double Discount { get; set; } = discount;
    public CappedPayBandThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a capped pay band threshold in status idle.</summary>
    public static CappedPayBandThreshold NewCappedPayBandThreshold(string code, string label)
    {
        return new CappedPayBandThreshold(code, label, 0, false, 0, "", 0.0, CappedPayBandThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayBandThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayBandThresholdStatus next)
    {
        switch (Status)
        {
            case CappedPayBandThresholdStatus.Idle:
                return next == CappedPayBandThresholdStatus.Ready;
            case CappedPayBandThresholdStatus.Warming:
                switch (next)
                {
                    case CappedPayBandThresholdStatus.Draining:
                    case CappedPayBandThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case CappedPayBandThresholdStatus.Ready:
                switch (next)
                {
                    case CappedPayBandThresholdStatus.Draining:
                    case CappedPayBandThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case CappedPayBandThresholdStatus.Draining:
                return next == CappedPayBandThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedPayBandThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pay band threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped pay band threshold"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Discount * (double)LeaveDays, 2);
    }

    public CappedPayBandThreshold Copy()
    {
        return new CappedPayBandThreshold(
            Code,
            Label,
            DurationMinutes,
            Approved,
            LeaveDays,
            CostCenter,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
