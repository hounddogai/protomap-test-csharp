namespace Acme.Workforce.Compliance.Exception.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception group is in its life cycle.</summary>
public enum ExceptionGroupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One exception group, with what the planners need to act on it.</summary>
public sealed class ExceptionGroup(string code, string title, string channel, bool active, string country, int capacity, int version, double threshold, ExceptionGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public bool Active { get; set; } = active;
    public string Country { get; set; } = country;
    public int Capacity { get; set; } = capacity;
    public int Version { get; set; } = version;
    public double Threshold { get; set; } = threshold;
    public ExceptionGroupStatus Status { get; set; } = status;

    /// <summary>Starts a exception group in status proposed.</summary>
    public static ExceptionGroup NewExceptionGroup(string code, string title)
    {
        return new ExceptionGroup(code, title, "", false, "", 0, 0, 0.0, ExceptionGroupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionGroupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionGroupStatus next)
    {
        switch (Status)
        {
            case ExceptionGroupStatus.Proposed:
                return next == ExceptionGroupStatus.Rejected;
            case ExceptionGroupStatus.Approved:
                switch (next)
                {
                    case ExceptionGroupStatus.Rejected:
                    case ExceptionGroupStatus.Withdrawn:
                        return true;
                }
                return false;
            case ExceptionGroupStatus.Rejected:
                switch (next)
                {
                    case ExceptionGroupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"exception group"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Threshold * (double)Capacity, 2);
    }

    public ExceptionGroup Copy()
    {
        return new ExceptionGroup(Code, Title, Channel, Active, Country, Capacity, Version, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
