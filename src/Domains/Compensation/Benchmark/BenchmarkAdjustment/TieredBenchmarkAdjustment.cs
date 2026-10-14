namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered benchmark adjustment is in its life cycle.</summary>
public enum TieredBenchmarkAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A tiered benchmark adjustment as the benchmark team keeps it.</summary>
public sealed class TieredBenchmarkAdjustment(string code, string name, double rating, List<string> tags, string notes, int startsAt, string contactEmail, double hours, TieredBenchmarkAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public List<string> Tags { get; set; } = tags;
    public string Notes { get; set; } = notes;
    public int StartsAt { get; set; } = startsAt;
    public string ContactEmail { get; set; } = contactEmail;
    public double Hours { get; set; } = hours;
    public TieredBenchmarkAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a tiered benchmark adjustment in status idle.</summary>
    public static TieredBenchmarkAdjustment NewTieredBenchmarkAdjustment(string code, string name)
    {
        return new TieredBenchmarkAdjustment(
            code,
            name,
            0.0,
            new List<string>(),
            "",
            0,
            "",
            0.0,
            TieredBenchmarkAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBenchmarkAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBenchmarkAdjustmentStatus next)
    {
        switch (Status)
        {
            case TieredBenchmarkAdjustmentStatus.Idle:
                return next == TieredBenchmarkAdjustmentStatus.Stopped;
            case TieredBenchmarkAdjustmentStatus.Warming:
                return next == TieredBenchmarkAdjustmentStatus.Stopped;
            case TieredBenchmarkAdjustmentStatus.Ready:
                return next == TieredBenchmarkAdjustmentStatus.Stopped;
            case TieredBenchmarkAdjustmentStatus.Draining:
                return next == TieredBenchmarkAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TieredBenchmarkAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered benchmark adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered benchmark adjustment"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedVolume()
    {
        return Math.Round(Rating + (double)StartsAt, 2);
    }

    public TieredBenchmarkAdjustment Copy()
    {
        return new TieredBenchmarkAdjustment(Code, Name, Rating, Tags, Notes, StartsAt, ContactEmail, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
