namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work metric is in its life cycle.</summary>
public enum StatementOfWorkMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One statement of work metric, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkMetric(string code, string title, string team, string segment, int version, double minimum, string costCenter, string source, int createdAt, StatementOfWorkMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Segment { get; set; } = segment;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public string CostCenter { get; set; } = costCenter;
    public string Source { get; set; } = source;
    public int CreatedAt { get; set; } = createdAt;
    public StatementOfWorkMetricStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work metric in status new.</summary>
    public static StatementOfWorkMetric NewStatementOfWorkMetric(string code, string title)
    {
        return new StatementOfWorkMetric(code, title, "", "", 0, 0.0, "", "", 0, StatementOfWorkMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkMetricStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkMetricStatus.New:
                return next == StatementOfWorkMetricStatus.Done || next == StatementOfWorkMetricStatus.Failed;
            case StatementOfWorkMetricStatus.Queued:
                switch (next)
                {
                    case StatementOfWorkMetricStatus.Failed:
                        return true;
                }
                return false;
            case StatementOfWorkMetricStatus.Running:
                switch (next)
                {
                    case StatementOfWorkMetricStatus.Done:
                    case StatementOfWorkMetricStatus.Failed:
                        return true;
                }
                return false;
            case StatementOfWorkMetricStatus.Done:
                return next == StatementOfWorkMetricStatus.Failed;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)Version * Minimum, 2);
    }

    public StatementOfWorkMetric Copy()
    {
        return new StatementOfWorkMetric(
            Code,
            Title,
            Team,
            Segment,
            Version,
            Minimum,
            CostCenter,
            Source,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
