namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped benchmark baseline is in its life cycle.</summary>
public enum CappedBenchmarkBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A capped benchmark baseline. Its code is unique within a tenant.</summary>
public sealed class CappedBenchmarkBaseline(string code, string label, int quantity, int version, double hours, double weight, double cost, int sequence, int windowDays, string externalRef, string jobCode, CappedBenchmarkBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int Version { get; set; } = version;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public int WindowDays { get; set; } = windowDays;
    public string ExternalRef { get; set; } = externalRef;
    public string JobCode { get; set; } = jobCode;
    public CappedBenchmarkBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a capped benchmark baseline in status created.</summary>
    public static CappedBenchmarkBaseline NewCappedBenchmarkBaseline(string code, string label)
    {
        return new CappedBenchmarkBaseline(
            code,
            label,
            0,
            0,
            0.0,
            0.0,
            0.0,
            0,
            0,
            "",
            "",
            CappedBenchmarkBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedBenchmarkBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedBenchmarkBaselineStatus next)
    {
        switch (Status)
        {
            case CappedBenchmarkBaselineStatus.Created:
                return next == CappedBenchmarkBaselineStatus.Retired;
            case CappedBenchmarkBaselineStatus.Validated:
                switch (next)
                {
                    case CappedBenchmarkBaselineStatus.Published:
                    case CappedBenchmarkBaselineStatus.Retired:
                        return true;
                }
                return false;
            case CappedBenchmarkBaselineStatus.Published:
                return next == CappedBenchmarkBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CappedBenchmarkBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped benchmark baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Cost - Weight, 2);
    }

    public CappedBenchmarkBaseline Copy()
    {
        return new CappedBenchmarkBaseline(
            Code,
            Label,
            Quantity,
            Version,
            Hours,
            Weight,
            Cost,
            Sequence,
            WindowDays,
            ExternalRef,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
