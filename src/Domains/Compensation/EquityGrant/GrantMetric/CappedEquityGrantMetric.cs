namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped equity grant metric is in its life cycle.</summary>
public enum CappedEquityGrantMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A capped equity grant metric. Its code is unique within a tenant.</summary>
public sealed class CappedEquityGrantMetric(string code, string label, int version, double amount, bool approved, string category, string jobCode, string source, double rating, CappedEquityGrantMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public double Amount { get; set; } = amount;
    public bool Approved { get; set; } = approved;
    public string Category { get; set; } = category;
    public string JobCode { get; set; } = jobCode;
    public string Source { get; set; } = source;
    public double Rating { get; set; } = rating;
    public CappedEquityGrantMetricStatus Status { get; set; } = status;

    /// <summary>Starts a capped equity grant metric in status created.</summary>
    public static CappedEquityGrantMetric NewCappedEquityGrantMetric(string code, string label)
    {
        return new CappedEquityGrantMetric(
            code,
            label,
            0,
            0.0,
            false,
            "",
            "",
            "",
            0.0,
            CappedEquityGrantMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedEquityGrantMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedEquityGrantMetricStatus next)
    {
        switch (Status)
        {
            case CappedEquityGrantMetricStatus.Created:
                switch (next)
                {
                    case CappedEquityGrantMetricStatus.Validated:
                        return true;
                }
                return false;
            case CappedEquityGrantMetricStatus.Validated:
                switch (next)
                {
                    case CappedEquityGrantMetricStatus.Retired:
                        return true;
                }
                return false;
            case CappedEquityGrantMetricStatus.Published:
                switch (next)
                {
                    case CappedEquityGrantMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedEquityGrantMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped equity grant metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Amount - (double)Version, 2);
    }

    public CappedEquityGrantMetric Copy()
    {
        return new CappedEquityGrantMetric(
            Code,
            Label,
            Version,
            Amount,
            Approved,
            Category,
            JobCode,
            Source,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
