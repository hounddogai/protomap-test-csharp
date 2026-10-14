namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global benchmark policy is in its life cycle.</summary>
public enum GlobalBenchmarkPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A global benchmark policy. Its code is unique within a tenant.</summary>
public sealed class GlobalBenchmarkPolicy(string code, string name, double fte, string category, double maximum, string costCenter, int limit, double threshold, string contactEmail, GlobalBenchmarkPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public string Category { get; set; } = category;
    public double Maximum { get; set; } = maximum;
    public string CostCenter { get; set; } = costCenter;
    public int Limit { get; set; } = limit;
    public double Threshold { get; set; } = threshold;
    public string ContactEmail { get; set; } = contactEmail;
    public GlobalBenchmarkPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a global benchmark policy in status new.</summary>
    public static GlobalBenchmarkPolicy NewGlobalBenchmarkPolicy(string code, string name)
    {
        return new GlobalBenchmarkPolicy(code, name, 0.0, "", 0.0, "", 0, 0.0, "", GlobalBenchmarkPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalBenchmarkPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalBenchmarkPolicyStatus next)
    {
        switch (Status)
        {
            case GlobalBenchmarkPolicyStatus.New:
                return next == GlobalBenchmarkPolicyStatus.Done || next == GlobalBenchmarkPolicyStatus.Failed;
            case GlobalBenchmarkPolicyStatus.Queued:
                switch (next)
                {
                    case GlobalBenchmarkPolicyStatus.Done:
                    case GlobalBenchmarkPolicyStatus.Failed:
                        return true;
                }
                return false;
            case GlobalBenchmarkPolicyStatus.Running:
                return next == GlobalBenchmarkPolicyStatus.Done || next == GlobalBenchmarkPolicyStatus.Failed;
            case GlobalBenchmarkPolicyStatus.Done:
                return next == GlobalBenchmarkPolicyStatus.Failed;
        }
        return false;
    }

    public void MoveTo(GlobalBenchmarkPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global benchmark policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Limit + Threshold, 2);
    }

    public GlobalBenchmarkPolicy Copy()
    {
        return new GlobalBenchmarkPolicy(
            Code,
            Name,
            Fte,
            Category,
            Maximum,
            CostCenter,
            Limit,
            Threshold,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
