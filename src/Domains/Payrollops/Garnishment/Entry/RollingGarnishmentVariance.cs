namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling garnishment variance is in its life cycle.</summary>
public enum RollingGarnishmentVarianceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One rolling garnishment variance, with what the planners need to act on it.</summary>
public sealed class RollingGarnishmentVariance(string code, string title, double hours, string description, string category, string source, RollingGarnishmentVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Description { get; set; } = description;
    public string Category { get; set; } = category;
    public string Source { get; set; } = source;
    public RollingGarnishmentVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a rolling garnishment variance in status idle.</summary>
    public static RollingGarnishmentVariance NewRollingGarnishmentVariance(string code, string title)
    {
        return new RollingGarnishmentVariance(code, title, 0.0, "", "", "", RollingGarnishmentVarianceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingGarnishmentVarianceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingGarnishmentVarianceStatus next)
    {
        switch (Status)
        {
            case RollingGarnishmentVarianceStatus.Idle:
                return next == RollingGarnishmentVarianceStatus.Warming || next == RollingGarnishmentVarianceStatus.Stopped;
            case RollingGarnishmentVarianceStatus.Warming:
                switch (next)
                {
                    case RollingGarnishmentVarianceStatus.Draining:
                        return true;
                }
                return false;
            case RollingGarnishmentVarianceStatus.Ready:
                switch (next)
                {
                    case RollingGarnishmentVarianceStatus.Draining:
                    case RollingGarnishmentVarianceStatus.Stopped:
                        return true;
                }
                return false;
            case RollingGarnishmentVarianceStatus.Draining:
                return next == RollingGarnishmentVarianceStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingGarnishmentVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling garnishment variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling garnishment variance"} {Code}";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public RollingGarnishmentVariance Copy()
    {
        return new RollingGarnishmentVariance(Code, Title, Hours, Description, Category, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
