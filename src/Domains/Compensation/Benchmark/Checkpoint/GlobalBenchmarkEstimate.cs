namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global benchmark estimate is in its life cycle.</summary>
public enum GlobalBenchmarkEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A global benchmark estimate. Its code is unique within a tenant.</summary>
public sealed class GlobalBenchmarkEstimate(string code, string name, List<string> tags, string employeeId, string contactEmail, int quantity, GlobalBenchmarkEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string EmployeeId { get; set; } = employeeId;
    public string ContactEmail { get; set; } = contactEmail;
    public int Quantity { get; set; } = quantity;
    public GlobalBenchmarkEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a global benchmark estimate in status proposed.</summary>
    public static GlobalBenchmarkEstimate NewGlobalBenchmarkEstimate(string code, string name)
    {
        return new GlobalBenchmarkEstimate(
            code,
            name,
            new List<string>(),
            "",
            "",
            0,
            GlobalBenchmarkEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalBenchmarkEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalBenchmarkEstimateStatus next)
    {
        switch (Status)
        {
            case GlobalBenchmarkEstimateStatus.Proposed:
                switch (next)
                {
                    case GlobalBenchmarkEstimateStatus.Approved:
                    case GlobalBenchmarkEstimateStatus.Rejected:
                        return true;
                }
                return false;
            case GlobalBenchmarkEstimateStatus.Approved:
                return next == GlobalBenchmarkEstimateStatus.Withdrawn;
            case GlobalBenchmarkEstimateStatus.Rejected:
                switch (next)
                {
                    case GlobalBenchmarkEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalBenchmarkEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global benchmark estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public GlobalBenchmarkEstimate Copy()
    {
        return new GlobalBenchmarkEstimate(Code, Name, Tags, EmployeeId, ContactEmail, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
