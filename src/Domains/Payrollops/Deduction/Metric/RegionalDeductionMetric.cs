namespace Acme.Workforce.Payrollops.Deduction.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional deduction metric is in its life cycle.</summary>
public enum RegionalDeductionMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional deduction metric. Its code is unique within a tenant.</summary>
public sealed class RegionalDeductionMetric(string code, string label, double weight, int retries, string department, string timezone, int capacity, double margin, string source, int expiresAt, int version, RegionalDeductionMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public int Retries { get; set; } = retries;
    public string Department { get; set; } = department;
    public string Timezone { get; set; } = timezone;
    public int Capacity { get; set; } = capacity;
    public double Margin { get; set; } = margin;
    public string Source { get; set; } = source;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Version { get; set; } = version;
    public RegionalDeductionMetricStatus Status { get; set; } = status;

    /// <summary>Starts a regional deduction metric in status created.</summary>
    public static RegionalDeductionMetric NewRegionalDeductionMetric(string code, string label)
    {
        return new RegionalDeductionMetric(
            code,
            label,
            0.0,
            0,
            "",
            "",
            0,
            0.0,
            "",
            0,
            0,
            RegionalDeductionMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalDeductionMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalDeductionMetricStatus next)
    {
        switch (Status)
        {
            case RegionalDeductionMetricStatus.Created:
                switch (next)
                {
                    case RegionalDeductionMetricStatus.Validated:
                    case RegionalDeductionMetricStatus.Published:
                        return true;
                }
                return false;
            case RegionalDeductionMetricStatus.Validated:
                switch (next)
                {
                    case RegionalDeductionMetricStatus.Published:
                    case RegionalDeductionMetricStatus.Retired:
                        return true;
                }
                return false;
            case RegionalDeductionMetricStatus.Published:
                return next == RegionalDeductionMetricStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalDeductionMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional deduction metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Weight * (double)ExpiresAt, 2);
    }

    public RegionalDeductionMetric Copy()
    {
        return new RegionalDeductionMetric(
            Code,
            Label,
            Weight,
            Retries,
            Department,
            Timezone,
            Capacity,
            Margin,
            Source,
            ExpiresAt,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
