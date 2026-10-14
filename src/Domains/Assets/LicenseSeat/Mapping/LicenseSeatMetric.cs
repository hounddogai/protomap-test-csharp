namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat metric is in its life cycle.</summary>
public enum LicenseSeatMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A license seat metric. Its code is unique within a tenant.</summary>
public sealed class LicenseSeatMetric(string code, string label, double floorValue, string jobCode, int graceDays, string contactEmail, double cost, int startsAt, LicenseSeatMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public string ContactEmail { get; set; } = contactEmail;
    public double Cost { get; set; } = cost;
    public int StartsAt { get; set; } = startsAt;
    public LicenseSeatMetricStatus Status { get; set; } = status;

    /// <summary>Starts a license seat metric in status new.</summary>
    public static LicenseSeatMetric NewLicenseSeatMetric(string code, string label)
    {
        return new LicenseSeatMetric(code, label, 0.0, "", 0, "", 0.0, 0, LicenseSeatMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatMetricStatus next)
    {
        switch (Status)
        {
            case LicenseSeatMetricStatus.New:
                return next == LicenseSeatMetricStatus.Running || next == LicenseSeatMetricStatus.Failed;
            case LicenseSeatMetricStatus.Queued:
                return next == LicenseSeatMetricStatus.Running || next == LicenseSeatMetricStatus.Done;
            case LicenseSeatMetricStatus.Running:
                return next == LicenseSeatMetricStatus.Done || next == LicenseSeatMetricStatus.Failed;
            case LicenseSeatMetricStatus.Done:
                return next == LicenseSeatMetricStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LicenseSeatMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)GraceDays - Cost, 2);
    }

    public LicenseSeatMetric Copy()
    {
        return new LicenseSeatMetric(
            Code,
            Label,
            FloorValue,
            JobCode,
            GraceDays,
            ContactEmail,
            Cost,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
