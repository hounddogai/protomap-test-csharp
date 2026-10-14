namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark preference is in its life cycle.</summary>
public enum BenchmarkPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A benchmark preference as the benchmark team keeps it.</summary>
public sealed class BenchmarkPreference(string code, string title, int tenureMonths, int startsAt, string grade, int priority, string phone, double rate, bool archived, double fte, BenchmarkPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public bool Archived { get; set; } = archived;
    public double Fte { get; set; } = fte;
    public BenchmarkPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark preference in status new.</summary>
    public static BenchmarkPreference NewBenchmarkPreference(string code, string title)
    {
        return new BenchmarkPreference(code, title, 0, 0, "", 0, "", 0.0, false, 0.0, BenchmarkPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkPreferenceStatus next)
    {
        switch (Status)
        {
            case BenchmarkPreferenceStatus.New:
                switch (next)
                {
                    case BenchmarkPreferenceStatus.Done:
                        return true;
                }
                return false;
            case BenchmarkPreferenceStatus.Queued:
                switch (next)
                {
                    case BenchmarkPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case BenchmarkPreferenceStatus.Running:
                return next == BenchmarkPreferenceStatus.Done || next == BenchmarkPreferenceStatus.Failed;
            case BenchmarkPreferenceStatus.Done:
                switch (next)
                {
                    case BenchmarkPreferenceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Rate + Fte, 2);
    }

    public BenchmarkPreference Copy()
    {
        return new BenchmarkPreference(
            Code,
            Title,
            TenureMonths,
            StartsAt,
            Grade,
            Priority,
            Phone,
            Rate,
            Archived,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
