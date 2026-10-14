namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped visitor window is in its life cycle.</summary>
public enum CappedVisitorWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A capped visitor window as the visitor team keeps it.</summary>
public sealed class CappedVisitorWindow(string code, string title, string region, string summary, string jobCode, int priority, double threshold, CappedVisitorWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Summary { get; set; } = summary;
    public string JobCode { get; set; } = jobCode;
    public int Priority { get; set; } = priority;
    public double Threshold { get; set; } = threshold;
    public CappedVisitorWindowStatus Status { get; set; } = status;

    /// <summary>Starts a capped visitor window in status draft.</summary>
    public static CappedVisitorWindow NewCappedVisitorWindow(string code, string title)
    {
        return new CappedVisitorWindow(code, title, "", "", "", 0, 0.0, CappedVisitorWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedVisitorWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedVisitorWindowStatus next)
    {
        switch (Status)
        {
            case CappedVisitorWindowStatus.Draft:
                return next == CappedVisitorWindowStatus.Active;
            case CappedVisitorWindowStatus.Pending:
                switch (next)
                {
                    case CappedVisitorWindowStatus.Suspended:
                    case CappedVisitorWindowStatus.Closed:
                        return true;
                }
                return false;
            case CappedVisitorWindowStatus.Active:
                switch (next)
                {
                    case CappedVisitorWindowStatus.Suspended:
                    case CappedVisitorWindowStatus.Closed:
                        return true;
                }
                return false;
            case CappedVisitorWindowStatus.Suspended:
                return next == CappedVisitorWindowStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedVisitorWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped visitor window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)Priority - Threshold, 2);
    }

    public CappedVisitorWindow Copy()
    {
        return new CappedVisitorWindow(Code, Title, Region, Summary, JobCode, Priority, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
