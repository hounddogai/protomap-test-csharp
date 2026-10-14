namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge request is in its life cycle.</summary>
public enum AccessBadgeRequestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A access badge request as the access badge team keeps it.</summary>
public sealed class AccessBadgeRequest(string code, string title, string jobCode, string name, string checksum, int quantity, AccessBadgeRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public int Quantity { get; set; } = quantity;
    public AccessBadgeRequestStatus Status { get; set; } = status;

    /// <summary>Starts a access badge request in status idle.</summary>
    public static AccessBadgeRequest NewAccessBadgeRequest(string code, string title)
    {
        return new AccessBadgeRequest(code, title, "", "", "", 0, AccessBadgeRequestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeRequestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeRequestStatus next)
    {
        switch (Status)
        {
            case AccessBadgeRequestStatus.Idle:
                switch (next)
                {
                    case AccessBadgeRequestStatus.Warming:
                        return true;
                }
                return false;
            case AccessBadgeRequestStatus.Warming:
                switch (next)
                {
                    case AccessBadgeRequestStatus.Ready:
                    case AccessBadgeRequestStatus.Draining:
                        return true;
                }
                return false;
            case AccessBadgeRequestStatus.Ready:
                return next == AccessBadgeRequestStatus.Stopped;
            case AccessBadgeRequestStatus.Draining:
                switch (next)
                {
                    case AccessBadgeRequestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public AccessBadgeRequest Copy()
    {
        return new AccessBadgeRequest(Code, Title, JobCode, Name, Checksum, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
