namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pay band group is in its life cycle.</summary>
public enum DefaultPayBandGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default pay band group. Its code is unique within a tenant.</summary>
public sealed class DefaultPayBandGroup(string code, string title, int tenureMonths, string employeeId, int retries, DefaultPayBandGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public DefaultPayBandGroupStatus Status { get; set; } = status;

    /// <summary>Starts a default pay band group in status idle.</summary>
    public static DefaultPayBandGroup NewDefaultPayBandGroup(string code, string title)
    {
        return new DefaultPayBandGroup(code, title, 0, "", 0, DefaultPayBandGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayBandGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayBandGroupStatus next)
    {
        switch (Status)
        {
            case DefaultPayBandGroupStatus.Idle:
                switch (next)
                {
                    case DefaultPayBandGroupStatus.Ready:
                    case DefaultPayBandGroupStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultPayBandGroupStatus.Warming:
                switch (next)
                {
                    case DefaultPayBandGroupStatus.Draining:
                    case DefaultPayBandGroupStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultPayBandGroupStatus.Ready:
                switch (next)
                {
                    case DefaultPayBandGroupStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultPayBandGroupStatus.Draining:
                switch (next)
                {
                    case DefaultPayBandGroupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPayBandGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pay band group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)TenureMonths - (double)Retries, 2);
    }

    public DefaultPayBandGroup Copy()
    {
        return new DefaultPayBandGroup(Code, Title, TenureMonths, EmployeeId, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
