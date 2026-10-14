namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary accrual request is in its life cycle.</summary>
public enum PrimaryAccrualRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary accrual request as the accrual team keeps it.</summary>
public sealed class PrimaryAccrualRequest(string code, string title, string channel, string notes, double amount, bool archived, int priority, string ownerId, string jobCode, PrimaryAccrualRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Notes { get; set; } = notes;
    public double Amount { get; set; } = amount;
    public bool Archived { get; set; } = archived;
    public int Priority { get; set; } = priority;
    public string OwnerId { get; set; } = ownerId;
    public string JobCode { get; set; } = jobCode;
    public PrimaryAccrualRequestStatus Status { get; set; } = status;

    /// <summary>Starts a primary accrual request in status requested.</summary>
    public static PrimaryAccrualRequest NewPrimaryAccrualRequest(string code, string title)
    {
        return new PrimaryAccrualRequest(
            code,
            title,
            "",
            "",
            0.0,
            false,
            0,
            "",
            "",
            PrimaryAccrualRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAccrualRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAccrualRequestStatus next)
    {
        switch (Status)
        {
            case PrimaryAccrualRequestStatus.Requested:
                switch (next)
                {
                    case PrimaryAccrualRequestStatus.Revoked:
                        return true;
                }
                return false;
            case PrimaryAccrualRequestStatus.Granted:
                return next == PrimaryAccrualRequestStatus.Revoked || next == PrimaryAccrualRequestStatus.Expired;
            case PrimaryAccrualRequestStatus.Revoked:
                return next == PrimaryAccrualRequestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PrimaryAccrualRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary accrual request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Priority - Amount, 2);
    }

    public PrimaryAccrualRequest Copy()
    {
        return new PrimaryAccrualRequest(
            Code,
            Title,
            Channel,
            Notes,
            Amount,
            Archived,
            Priority,
            OwnerId,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
