namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global benchmark request is in its life cycle.</summary>
public enum GlobalBenchmarkRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global benchmark request. Its code is unique within a tenant.</summary>
public sealed class GlobalBenchmarkRequest(string code, string label, string locale, string managerId, double budget, double score, string checksum, string notes, string jobCode, double maximum, double minimum, GlobalBenchmarkRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string ManagerId { get; set; } = managerId;
    public double Budget { get; set; } = budget;
    public double Score { get; set; } = score;
    public string Checksum { get; set; } = checksum;
    public string Notes { get; set; } = notes;
    public string JobCode { get; set; } = jobCode;
    public double Maximum { get; set; } = maximum;
    public double Minimum { get; set; } = minimum;
    public GlobalBenchmarkRequestStatus Status { get; set; } = status;

    /// <summary>Starts a global benchmark request in status planned.</summary>
    public static GlobalBenchmarkRequest NewGlobalBenchmarkRequest(string code, string label)
    {
        return new GlobalBenchmarkRequest(
            code,
            label,
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            "",
            0.0,
            0.0,
            GlobalBenchmarkRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalBenchmarkRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalBenchmarkRequestStatus next)
    {
        switch (Status)
        {
            case GlobalBenchmarkRequestStatus.Planned:
                return next == GlobalBenchmarkRequestStatus.Cancelled;
            case GlobalBenchmarkRequestStatus.Scheduled:
                return next == GlobalBenchmarkRequestStatus.Completed;
            case GlobalBenchmarkRequestStatus.InProgress:
                return next == GlobalBenchmarkRequestStatus.Completed || next == GlobalBenchmarkRequestStatus.Cancelled;
            case GlobalBenchmarkRequestStatus.Completed:
                switch (next)
                {
                    case GlobalBenchmarkRequestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalBenchmarkRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global benchmark request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Maximum - Minimum, 2);
    }

    public GlobalBenchmarkRequest Copy()
    {
        return new GlobalBenchmarkRequest(
            Code,
            Label,
            Locale,
            ManagerId,
            Budget,
            Score,
            Checksum,
            Notes,
            JobCode,
            Maximum,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
