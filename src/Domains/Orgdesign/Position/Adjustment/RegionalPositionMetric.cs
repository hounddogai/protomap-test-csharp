namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional position metric is in its life cycle.</summary>
public enum RegionalPositionMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a regional position metric and its current status.</summary>
public sealed class RegionalPositionMetric(string code, string title, double discount, string timezone, int graceDays, string salaryBand, int sequence, RegionalPositionMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Timezone { get; set; } = timezone;
    public int GraceDays { get; set; } = graceDays;
    public string SalaryBand { get; set; } = salaryBand;
    public int Sequence { get; set; } = sequence;
    public RegionalPositionMetricStatus Status { get; set; } = status;

    /// <summary>Starts a regional position metric in status idle.</summary>
    public static RegionalPositionMetric NewRegionalPositionMetric(string code, string title)
    {
        return new RegionalPositionMetric(code, title, 0.0, "", 0, "", 0, RegionalPositionMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPositionMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPositionMetricStatus next)
    {
        switch (Status)
        {
            case RegionalPositionMetricStatus.Idle:
                return next == RegionalPositionMetricStatus.Ready;
            case RegionalPositionMetricStatus.Warming:
                return next == RegionalPositionMetricStatus.Draining;
            case RegionalPositionMetricStatus.Ready:
                return next == RegionalPositionMetricStatus.Draining || next == RegionalPositionMetricStatus.Stopped;
            case RegionalPositionMetricStatus.Draining:
                switch (next)
                {
                    case RegionalPositionMetricStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalPositionMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional position metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Discount - (double)Sequence, 2);
    }

    public RegionalPositionMetric Copy()
    {
        return new RegionalPositionMetric(Code, Title, Discount, Timezone, GraceDays, SalaryBand, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
