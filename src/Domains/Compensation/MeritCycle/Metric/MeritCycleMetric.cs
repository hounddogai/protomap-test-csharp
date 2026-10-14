namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle metric is in its life cycle.</summary>
public enum MeritCycleMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One merit cycle metric, with what the planners need to act on it.</summary>
public sealed class MeritCycleMetric(string code, string name, string country, string contactEmail, string description, double rate, int limit, string externalRef, MeritCycleMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string ContactEmail { get; set; } = contactEmail;
    public string Description { get; set; } = description;
    public double Rate { get; set; } = rate;
    public int Limit { get; set; } = limit;
    public string ExternalRef { get; set; } = externalRef;
    public MeritCycleMetricStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle metric in status open.</summary>
    public static MeritCycleMetric NewMeritCycleMetric(string code, string name)
    {
        return new MeritCycleMetric(code, name, "", "", "", 0.0, 0, "", MeritCycleMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleMetricStatus next)
    {
        switch (Status)
        {
            case MeritCycleMetricStatus.Open:
                switch (next)
                {
                    case MeritCycleMetricStatus.Reopened:
                    case MeritCycleMetricStatus.Archived:
                        return true;
                }
                return false;
            case MeritCycleMetricStatus.InReview:
                return next == MeritCycleMetricStatus.Reopened || next == MeritCycleMetricStatus.Archived;
            case MeritCycleMetricStatus.Resolved:
                switch (next)
                {
                    case MeritCycleMetricStatus.Reopened:
                        return true;
                }
                return false;
            case MeritCycleMetricStatus.Reopened:
                return next == MeritCycleMetricStatus.Archived;
        }
        return false;
    }

    public void MoveTo(MeritCycleMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"merit cycle metric"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Rate - (double)Limit, 2);
    }

    public MeritCycleMetric Copy()
    {
        return new MeritCycleMetric(
            Code,
            Name,
            Country,
            ContactEmail,
            Description,
            Rate,
            Limit,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
