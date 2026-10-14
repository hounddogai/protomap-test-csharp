namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual reorg metric is in its life cycle.</summary>
public enum ManualReorgMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One manual reorg metric, with what the planners need to act on it.</summary>
public sealed class ManualReorgMetric(string code, string title, int version, double ceiling, double ratio, int attempts, ManualReorgMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public double Ceiling { get; set; } = ceiling;
    public double Ratio { get; set; } = ratio;
    public int Attempts { get; set; } = attempts;
    public ManualReorgMetricStatus Status { get; set; } = status;

    /// <summary>Starts a manual reorg metric in status planned.</summary>
    public static ManualReorgMetric NewManualReorgMetric(string code, string title)
    {
        return new ManualReorgMetric(code, title, 0, 0.0, 0.0, 0, ManualReorgMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualReorgMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualReorgMetricStatus next)
    {
        switch (Status)
        {
            case ManualReorgMetricStatus.Planned:
                switch (next)
                {
                    case ManualReorgMetricStatus.InProgress:
                        return true;
                }
                return false;
            case ManualReorgMetricStatus.Scheduled:
                switch (next)
                {
                    case ManualReorgMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualReorgMetricStatus.InProgress:
                return next == ManualReorgMetricStatus.Completed || next == ManualReorgMetricStatus.Cancelled;
            case ManualReorgMetricStatus.Completed:
                switch (next)
                {
                    case ManualReorgMetricStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualReorgMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual reorg metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Ceiling - (double)Attempts, 2);
    }

    public ManualReorgMetric Copy()
    {
        return new ManualReorgMetric(Code, Title, Version, Ceiling, Ratio, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
