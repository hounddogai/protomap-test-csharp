namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global benchmark profile is in its life cycle.</summary>
public enum GlobalBenchmarkProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A global benchmark profile as the benchmark team keeps it.</summary>
public sealed class GlobalBenchmarkProfile(string code, string label, string ownerId, string employeeId, string contactEmail, double fte, double hours, int durationMinutes, GlobalBenchmarkProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string EmployeeId { get; set; } = employeeId;
    public string ContactEmail { get; set; } = contactEmail;
    public double Fte { get; set; } = fte;
    public double Hours { get; set; } = hours;
    public int DurationMinutes { get; set; } = durationMinutes;
    public GlobalBenchmarkProfileStatus Status { get; set; } = status;

    /// <summary>Starts a global benchmark profile in status new.</summary>
    public static GlobalBenchmarkProfile NewGlobalBenchmarkProfile(string code, string label)
    {
        return new GlobalBenchmarkProfile(code, label, "", "", "", 0.0, 0.0, 0, GlobalBenchmarkProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalBenchmarkProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalBenchmarkProfileStatus next)
    {
        switch (Status)
        {
            case GlobalBenchmarkProfileStatus.New:
                switch (next)
                {
                    case GlobalBenchmarkProfileStatus.Running:
                        return true;
                }
                return false;
            case GlobalBenchmarkProfileStatus.Queued:
                switch (next)
                {
                    case GlobalBenchmarkProfileStatus.Failed:
                        return true;
                }
                return false;
            case GlobalBenchmarkProfileStatus.Running:
                switch (next)
                {
                    case GlobalBenchmarkProfileStatus.Done:
                    case GlobalBenchmarkProfileStatus.Failed:
                        return true;
                }
                return false;
            case GlobalBenchmarkProfileStatus.Done:
                switch (next)
                {
                    case GlobalBenchmarkProfileStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalBenchmarkProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global benchmark profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)DurationMinutes + Hours, 2);
    }

    public GlobalBenchmarkProfile Copy()
    {
        return new GlobalBenchmarkProfile(
            Code,
            Label,
            OwnerId,
            EmployeeId,
            ContactEmail,
            Fte,
            Hours,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
