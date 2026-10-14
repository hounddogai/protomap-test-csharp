namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool summary is in its life cycle.</summary>
public enum PoolSummaryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pool summary. Its code is unique within a tenant.</summary>
public sealed class PoolSummary(string code, string name, int retries, double threshold, string locale, string badge, string checksum, PoolSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public string Locale { get; set; } = locale;
    public string Badge { get; set; } = badge;
    public string Checksum { get; set; } = checksum;
    public PoolSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a pool summary in status planned.</summary>
    public static PoolSummary NewPoolSummary(string code, string name)
    {
        return new PoolSummary(code, name, 0, 0.0, "", "", "", PoolSummaryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolSummaryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolSummaryStatus next)
    {
        switch (Status)
        {
            case PoolSummaryStatus.Planned:
                switch (next)
                {
                    case PoolSummaryStatus.InProgress:
                    case PoolSummaryStatus.Cancelled:
                        return true;
                }
                return false;
            case PoolSummaryStatus.Scheduled:
                return next == PoolSummaryStatus.InProgress;
            case PoolSummaryStatus.InProgress:
                switch (next)
                {
                    case PoolSummaryStatus.Completed:
                        return true;
                }
                return false;
            case PoolSummaryStatus.Completed:
                switch (next)
                {
                    case PoolSummaryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Threshold - (double)Retries, 2);
    }

    public PoolSummary Copy()
    {
        return new PoolSummary(Code, Name, Retries, Threshold, Locale, Badge, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
