namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive benchmark metric is in its life cycle.</summary>
public enum AdaptiveBenchmarkMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a adaptive benchmark metric and its current status.</summary>
public sealed class AdaptiveBenchmarkMetric(string code, string label, string location, bool approved, double fte, string ownerId, string skill, AdaptiveBenchmarkMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public bool Approved { get; set; } = approved;
    public double Fte { get; set; } = fte;
    public string OwnerId { get; set; } = ownerId;
    public string Skill { get; set; } = skill;
    public AdaptiveBenchmarkMetricStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive benchmark metric in status open.</summary>
    public static AdaptiveBenchmarkMetric NewAdaptiveBenchmarkMetric(string code, string label)
    {
        return new AdaptiveBenchmarkMetric(code, label, "", false, 0.0, "", "", AdaptiveBenchmarkMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBenchmarkMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBenchmarkMetricStatus next)
    {
        switch (Status)
        {
            case AdaptiveBenchmarkMetricStatus.Open:
                switch (next)
                {
                    case AdaptiveBenchmarkMetricStatus.Reopened:
                        return true;
                }
                return false;
            case AdaptiveBenchmarkMetricStatus.InReview:
                switch (next)
                {
                    case AdaptiveBenchmarkMetricStatus.Reopened:
                    case AdaptiveBenchmarkMetricStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveBenchmarkMetricStatus.Resolved:
                return next == AdaptiveBenchmarkMetricStatus.Reopened;
            case AdaptiveBenchmarkMetricStatus.Reopened:
                switch (next)
                {
                    case AdaptiveBenchmarkMetricStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveBenchmarkMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive benchmark metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public AdaptiveBenchmarkMetric Copy()
    {
        return new AdaptiveBenchmarkMetric(Code, Label, Location, Approved, Fte, OwnerId, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
