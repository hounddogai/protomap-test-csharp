namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool exception is in its life cycle.</summary>
public enum PoolExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pool exception. Its code is unique within a tenant.</summary>
public sealed class PoolException(string code, string title, string timezone, string employeeId, string description, int quantity, PoolExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string EmployeeId { get; set; } = employeeId;
    public string Description { get; set; } = description;
    public int Quantity { get; set; } = quantity;
    public PoolExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a pool exception in status new.</summary>
    public static PoolException NewPoolException(string code, string title)
    {
        return new PoolException(code, title, "", "", "", 0, PoolExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolExceptionStatus next)
    {
        switch (Status)
        {
            case PoolExceptionStatus.New:
                switch (next)
                {
                    case PoolExceptionStatus.Queued:
                        return true;
                }
                return false;
            case PoolExceptionStatus.Queued:
                return next == PoolExceptionStatus.Failed;
            case PoolExceptionStatus.Running:
                switch (next)
                {
                    case PoolExceptionStatus.Done:
                    case PoolExceptionStatus.Failed:
                        return true;
                }
                return false;
            case PoolExceptionStatus.Done:
                switch (next)
                {
                    case PoolExceptionStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public PoolException Copy()
    {
        return new PoolException(Code, Title, Timezone, EmployeeId, Description, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
