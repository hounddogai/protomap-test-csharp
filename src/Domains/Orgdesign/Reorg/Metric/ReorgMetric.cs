namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg metric is in its life cycle.</summary>
public enum ReorgMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a reorg metric and its current status.</summary>
public sealed class ReorgMetric(string code, string title, int batchSize, string locale, string description, ReorgMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Locale { get; set; } = locale;
    public string Description { get; set; } = description;
    public ReorgMetricStatus Status { get; set; } = status;

    /// <summary>Starts a reorg metric in status draft.</summary>
    public static ReorgMetric NewReorgMetric(string code, string title)
    {
        return new ReorgMetric(code, title, 0, "", "", ReorgMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgMetricStatus next)
    {
        switch (Status)
        {
            case ReorgMetricStatus.Draft:
                return next == ReorgMetricStatus.Suspended;
            case ReorgMetricStatus.Pending:
                return next == ReorgMetricStatus.Active || next == ReorgMetricStatus.Suspended;
            case ReorgMetricStatus.Active:
                return next == ReorgMetricStatus.Closed;
            case ReorgMetricStatus.Suspended:
                switch (next)
                {
                    case ReorgMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public ReorgMetric Copy()
    {
        return new ReorgMetric(Code, Title, BatchSize, Locale, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
