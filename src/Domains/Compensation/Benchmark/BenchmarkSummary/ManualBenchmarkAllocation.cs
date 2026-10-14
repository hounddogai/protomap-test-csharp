namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual benchmark allocation is in its life cycle.</summary>
public enum ManualBenchmarkAllocationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A manual benchmark allocation. Its code is unique within a tenant.</summary>
public sealed class ManualBenchmarkAllocation(string code, string label, string summary, string location, double cost, string skill, string grade, int retries, int startsAt, string timezone, ManualBenchmarkAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public string Location { get; set; } = location;
    public double Cost { get; set; } = cost;
    public string Skill { get; set; } = skill;
    public string Grade { get; set; } = grade;
    public int Retries { get; set; } = retries;
    public int StartsAt { get; set; } = startsAt;
    public string Timezone { get; set; } = timezone;
    public ManualBenchmarkAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a manual benchmark allocation in status draft.</summary>
    public static ManualBenchmarkAllocation NewManualBenchmarkAllocation(string code, string label)
    {
        return new ManualBenchmarkAllocation(
            code,
            label,
            "",
            "",
            0.0,
            "",
            "",
            0,
            0,
            "",
            ManualBenchmarkAllocationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBenchmarkAllocationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBenchmarkAllocationStatus next)
    {
        switch (Status)
        {
            case ManualBenchmarkAllocationStatus.Draft:
                switch (next)
                {
                    case ManualBenchmarkAllocationStatus.Pending:
                        return true;
                }
                return false;
            case ManualBenchmarkAllocationStatus.Pending:
                return next == ManualBenchmarkAllocationStatus.Active || next == ManualBenchmarkAllocationStatus.Closed;
            case ManualBenchmarkAllocationStatus.Active:
                switch (next)
                {
                    case ManualBenchmarkAllocationStatus.Suspended:
                        return true;
                }
                return false;
            case ManualBenchmarkAllocationStatus.Suspended:
                return next == ManualBenchmarkAllocationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualBenchmarkAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual benchmark allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetLoad()
    {
        return Math.Round(Cost * (double)StartsAt, 2);
    }

    public ManualBenchmarkAllocation Copy()
    {
        return new ManualBenchmarkAllocation(
            Code,
            Label,
            Summary,
            Location,
            Cost,
            Skill,
            Grade,
            Retries,
            StartsAt,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
