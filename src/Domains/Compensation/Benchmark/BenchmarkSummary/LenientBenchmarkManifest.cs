namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient benchmark manifest is in its life cycle.</summary>
public enum LenientBenchmarkManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient benchmark manifest as the benchmark team keeps it.</summary>
public sealed class LenientBenchmarkManifest(string code, string name, double threshold, int endsAt, Dictionary<string, string> attributes, string employeeId, string source, LenientBenchmarkManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public int EndsAt { get; set; } = endsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string EmployeeId { get; set; } = employeeId;
    public string Source { get; set; } = source;
    public LenientBenchmarkManifestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient benchmark manifest in status idle.</summary>
    public static LenientBenchmarkManifest NewLenientBenchmarkManifest(string code, string name)
    {
        return new LenientBenchmarkManifest(
            code,
            name,
            0.0,
            0,
            new Dictionary<string, string>(),
            "",
            "",
            LenientBenchmarkManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBenchmarkManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBenchmarkManifestStatus next)
    {
        switch (Status)
        {
            case LenientBenchmarkManifestStatus.Idle:
                return next == LenientBenchmarkManifestStatus.Stopped;
            case LenientBenchmarkManifestStatus.Warming:
                switch (next)
                {
                    case LenientBenchmarkManifestStatus.Draining:
                    case LenientBenchmarkManifestStatus.Stopped:
                        return true;
                }
                return false;
            case LenientBenchmarkManifestStatus.Ready:
                return next == LenientBenchmarkManifestStatus.Draining || next == LenientBenchmarkManifestStatus.Stopped;
            case LenientBenchmarkManifestStatus.Draining:
                switch (next)
                {
                    case LenientBenchmarkManifestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBenchmarkManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient benchmark manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveVolume()
    {
        return Math.Round(Threshold + (double)EndsAt, 2);
    }

    public LenientBenchmarkManifest Copy()
    {
        return new LenientBenchmarkManifest(Code, Name, Threshold, EndsAt, Attributes, EmployeeId, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
