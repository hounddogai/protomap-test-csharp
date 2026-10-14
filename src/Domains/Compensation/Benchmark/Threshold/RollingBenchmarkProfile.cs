namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling benchmark profile is in its life cycle.</summary>
public enum RollingBenchmarkProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rolling benchmark profile as the benchmark team keeps it.</summary>
public sealed class RollingBenchmarkProfile(string code, string title, Dictionary<string, string> attributes, string ownerId, string contactEmail, int leaveDays, double amount, int priority, string country, RollingBenchmarkProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string OwnerId { get; set; } = ownerId;
    public string ContactEmail { get; set; } = contactEmail;
    public int LeaveDays { get; set; } = leaveDays;
    public double Amount { get; set; } = amount;
    public int Priority { get; set; } = priority;
    public string Country { get; set; } = country;
    public RollingBenchmarkProfileStatus Status { get; set; } = status;

    /// <summary>Starts a rolling benchmark profile in status idle.</summary>
    public static RollingBenchmarkProfile NewRollingBenchmarkProfile(string code, string title)
    {
        return new RollingBenchmarkProfile(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            "",
            0,
            0.0,
            0,
            "",
            RollingBenchmarkProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBenchmarkProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBenchmarkProfileStatus next)
    {
        switch (Status)
        {
            case RollingBenchmarkProfileStatus.Idle:
                return next == RollingBenchmarkProfileStatus.Draining;
            case RollingBenchmarkProfileStatus.Warming:
                switch (next)
                {
                    case RollingBenchmarkProfileStatus.Stopped:
                        return true;
                }
                return false;
            case RollingBenchmarkProfileStatus.Ready:
                switch (next)
                {
                    case RollingBenchmarkProfileStatus.Stopped:
                        return true;
                }
                return false;
            case RollingBenchmarkProfileStatus.Draining:
                return next == RollingBenchmarkProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingBenchmarkProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling benchmark profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling benchmark profile"} {Code}";
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

    public double WeightedLoad()
    {
        return Math.Round((double)LeaveDays - (double)Priority, 2);
    }

    public RollingBenchmarkProfile Copy()
    {
        return new RollingBenchmarkProfile(
            Code,
            Title,
            Attributes,
            OwnerId,
            ContactEmail,
            LeaveDays,
            Amount,
            Priority,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
