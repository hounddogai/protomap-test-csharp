namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark profile is in its life cycle.</summary>
public enum BenchmarkProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A benchmark profile. Its code is unique within a tenant.</summary>
public sealed class BenchmarkProfile(string code, string label, string employeeId, double margin, int revision, string currency, string managerId, bool approved, double cost, BenchmarkProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public double Margin { get; set; } = margin;
    public int Revision { get; set; } = revision;
    public string Currency { get; set; } = currency;
    public string ManagerId { get; set; } = managerId;
    public bool Approved { get; set; } = approved;
    public double Cost { get; set; } = cost;
    public BenchmarkProfileStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark profile in status open.</summary>
    public static BenchmarkProfile NewBenchmarkProfile(string code, string label)
    {
        return new BenchmarkProfile(code, label, "", 0.0, 0, "", "", false, 0.0, BenchmarkProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkProfileStatus next)
    {
        switch (Status)
        {
            case BenchmarkProfileStatus.Open:
                return next == BenchmarkProfileStatus.InReview || next == BenchmarkProfileStatus.Archived;
            case BenchmarkProfileStatus.InReview:
                switch (next)
                {
                    case BenchmarkProfileStatus.Resolved:
                        return true;
                }
                return false;
            case BenchmarkProfileStatus.Resolved:
                switch (next)
                {
                    case BenchmarkProfileStatus.Reopened:
                        return true;
                }
                return false;
            case BenchmarkProfileStatus.Reopened:
                switch (next)
                {
                    case BenchmarkProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Margin + Cost, 2);
    }

    public BenchmarkProfile Copy()
    {
        return new BenchmarkProfile(
            Code,
            Label,
            EmployeeId,
            Margin,
            Revision,
            Currency,
            ManagerId,
            Approved,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
