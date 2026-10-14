namespace Acme.Workforce.Workplace.Visitor.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor window is in its life cycle.</summary>
public enum VisitorWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One visitor window, with what the planners need to act on it.</summary>
public sealed class VisitorWindow(string code, string name, string externalRef, string employeeId, int capacity, string segment, int limit, string region, double score, VisitorWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public string EmployeeId { get; set; } = employeeId;
    public int Capacity { get; set; } = capacity;
    public string Segment { get; set; } = segment;
    public int Limit { get; set; } = limit;
    public string Region { get; set; } = region;
    public double Score { get; set; } = score;
    public VisitorWindowStatus Status { get; set; } = status;

    /// <summary>Starts a visitor window in status requested.</summary>
    public static VisitorWindow NewVisitorWindow(string code, string name)
    {
        return new VisitorWindow(code, name, "", "", 0, "", 0, "", 0.0, VisitorWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorWindowStatus next)
    {
        switch (Status)
        {
            case VisitorWindowStatus.Requested:
                switch (next)
                {
                    case VisitorWindowStatus.Granted:
                        return true;
                }
                return false;
            case VisitorWindowStatus.Granted:
                return next == VisitorWindowStatus.Expired;
            case VisitorWindowStatus.Revoked:
                return next == VisitorWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(VisitorWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Capacity - (double)Limit, 2);
    }

    public VisitorWindow Copy()
    {
        return new VisitorWindow(
            Code,
            Name,
            ExternalRef,
            EmployeeId,
            Capacity,
            Segment,
            Limit,
            Region,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
