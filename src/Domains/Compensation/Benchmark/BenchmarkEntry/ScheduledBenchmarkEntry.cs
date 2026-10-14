namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled benchmark entry is in its life cycle.</summary>
public enum ScheduledBenchmarkEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled benchmark entry. Its code is unique within a tenant.</summary>
public sealed class ScheduledBenchmarkEntry(string code, string label, string phone, int retries, string reviewer, ScheduledBenchmarkEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public int Retries { get; set; } = retries;
    public string Reviewer { get; set; } = reviewer;
    public ScheduledBenchmarkEntryStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled benchmark entry in status requested.</summary>
    public static ScheduledBenchmarkEntry NewScheduledBenchmarkEntry(string code, string label)
    {
        return new ScheduledBenchmarkEntry(code, label, "", 0, "", ScheduledBenchmarkEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBenchmarkEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBenchmarkEntryStatus next)
    {
        switch (Status)
        {
            case ScheduledBenchmarkEntryStatus.Requested:
                switch (next)
                {
                    case ScheduledBenchmarkEntryStatus.Expired:
                        return true;
                }
                return false;
            case ScheduledBenchmarkEntryStatus.Granted:
                return next == ScheduledBenchmarkEntryStatus.Revoked;
            case ScheduledBenchmarkEntryStatus.Revoked:
                return next == ScheduledBenchmarkEntryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ScheduledBenchmarkEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled benchmark entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled benchmark entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public ScheduledBenchmarkEntry Copy()
    {
        return new ScheduledBenchmarkEntry(Code, Label, Phone, Retries, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
