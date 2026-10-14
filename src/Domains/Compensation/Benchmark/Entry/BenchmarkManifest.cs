namespace Acme.Workforce.Compensation.Benchmark.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark manifest is in its life cycle.</summary>
public enum BenchmarkManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One benchmark manifest, with what the planners need to act on it.</summary>
public sealed class BenchmarkManifest(string code, string label, double floorValue, string grade, bool visible, double maximum, string externalRef, BenchmarkManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Grade { get; set; } = grade;
    public bool Visible { get; set; } = visible;
    public double Maximum { get; set; } = maximum;
    public string ExternalRef { get; set; } = externalRef;
    public BenchmarkManifestStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark manifest in status created.</summary>
    public static BenchmarkManifest NewBenchmarkManifest(string code, string label)
    {
        return new BenchmarkManifest(code, label, 0.0, "", false, 0.0, "", BenchmarkManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkManifestStatus next)
    {
        switch (Status)
        {
            case BenchmarkManifestStatus.Created:
                return next == BenchmarkManifestStatus.Retired;
            case BenchmarkManifestStatus.Validated:
                return next == BenchmarkManifestStatus.Retired;
            case BenchmarkManifestStatus.Published:
                return next == BenchmarkManifestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BenchmarkManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Maximum * FloorValue, 2);
    }

    public BenchmarkManifest Copy()
    {
        return new BenchmarkManifest(Code, Label, FloorValue, Grade, Visible, Maximum, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
