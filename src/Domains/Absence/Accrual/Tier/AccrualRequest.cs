namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual request is in its life cycle.</summary>
public enum AccrualRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A accrual request as the accrual team keeps it.</summary>
public sealed class AccrualRequest(string code, string name, double ceiling, int version, int quantity, AccrualRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public int Version { get; set; } = version;
    public int Quantity { get; set; } = quantity;
    public AccrualRequestStatus Status { get; set; } = status;

    /// <summary>Starts a accrual request in status open.</summary>
    public static AccrualRequest NewAccrualRequest(string code, string name)
    {
        return new AccrualRequest(code, name, 0.0, 0, 0, AccrualRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualRequestStatus next)
    {
        switch (Status)
        {
            case AccrualRequestStatus.Open:
                switch (next)
                {
                    case AccrualRequestStatus.InReview:
                    case AccrualRequestStatus.Reopened:
                        return true;
                }
                return false;
            case AccrualRequestStatus.InReview:
                switch (next)
                {
                    case AccrualRequestStatus.Reopened:
                        return true;
                }
                return false;
            case AccrualRequestStatus.Resolved:
                return next == AccrualRequestStatus.Reopened || next == AccrualRequestStatus.Archived;
            case AccrualRequestStatus.Reopened:
                switch (next)
                {
                    case AccrualRequestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)Quantity - Ceiling, 2);
    }

    public AccrualRequest Copy()
    {
        return new AccrualRequest(Code, Name, Ceiling, Version, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
