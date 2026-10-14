namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding metric is in its life cycle.</summary>
public enum OffboardingMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A offboarding metric as the offboarding team keeps it.</summary>
public sealed class OffboardingMetric(string code, string name, int limit, double weight, double rate, int createdAt, int endsAt, double rating, OffboardingMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public double Weight { get; set; } = weight;
    public double Rate { get; set; } = rate;
    public int CreatedAt { get; set; } = createdAt;
    public int EndsAt { get; set; } = endsAt;
    public double Rating { get; set; } = rating;
    public OffboardingMetricStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding metric in status new.</summary>
    public static OffboardingMetric NewOffboardingMetric(string code, string name)
    {
        return new OffboardingMetric(code, name, 0, 0.0, 0.0, 0, 0, 0.0, OffboardingMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingMetricStatus next)
    {
        switch (Status)
        {
            case OffboardingMetricStatus.New:
                switch (next)
                {
                    case OffboardingMetricStatus.Failed:
                        return true;
                }
                return false;
            case OffboardingMetricStatus.Queued:
                return next == OffboardingMetricStatus.Running;
            case OffboardingMetricStatus.Running:
                return next == OffboardingMetricStatus.Done;
            case OffboardingMetricStatus.Done:
                switch (next)
                {
                    case OffboardingMetricStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Rating + (double)EndsAt, 2);
    }

    public OffboardingMetric Copy()
    {
        return new OffboardingMetric(Code, Name, Limit, Weight, Rate, CreatedAt, EndsAt, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
