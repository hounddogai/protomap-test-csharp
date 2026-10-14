namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default engagement exception is in its life cycle.</summary>
public enum DefaultEngagementExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One default engagement exception, with what the planners need to act on it.</summary>
public sealed class DefaultEngagementException(string code, string name, int capacity, int quantity, string summary, string managerId, double ratio, string currency, double rate, DefaultEngagementExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int Quantity { get; set; } = quantity;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public double Ratio { get; set; } = ratio;
    public string Currency { get; set; } = currency;
    public double Rate { get; set; } = rate;
    public DefaultEngagementExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a default engagement exception in status new.</summary>
    public static DefaultEngagementException NewDefaultEngagementException(string code, string name)
    {
        return new DefaultEngagementException(
            code,
            name,
            0,
            0,
            "",
            "",
            0.0,
            "",
            0.0,
            DefaultEngagementExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultEngagementExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultEngagementExceptionStatus next)
    {
        switch (Status)
        {
            case DefaultEngagementExceptionStatus.New:
                switch (next)
                {
                    case DefaultEngagementExceptionStatus.Queued:
                    case DefaultEngagementExceptionStatus.Failed:
                        return true;
                }
                return false;
            case DefaultEngagementExceptionStatus.Queued:
                return next == DefaultEngagementExceptionStatus.Failed;
            case DefaultEngagementExceptionStatus.Running:
                switch (next)
                {
                    case DefaultEngagementExceptionStatus.Failed:
                        return true;
                }
                return false;
            case DefaultEngagementExceptionStatus.Done:
                return next == DefaultEngagementExceptionStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DefaultEngagementExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default engagement exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rate * (double)Capacity, 2);
    }

    public DefaultEngagementException Copy()
    {
        return new DefaultEngagementException(
            Code,
            Name,
            Capacity,
            Quantity,
            Summary,
            ManagerId,
            Ratio,
            Currency,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
