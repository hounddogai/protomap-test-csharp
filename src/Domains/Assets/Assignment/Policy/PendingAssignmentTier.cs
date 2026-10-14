namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending assignment tier is in its life cycle.</summary>
public enum PendingAssignmentTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a pending assignment tier and its current status.</summary>
public sealed class PendingAssignmentTier(string code, string title, int retries, string ownerId, string currency, string managerId, PendingAssignmentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string OwnerId { get; set; } = ownerId;
    public string Currency { get; set; } = currency;
    public string ManagerId { get; set; } = managerId;
    public PendingAssignmentTierStatus Status { get; set; } = status;

    /// <summary>Starts a pending assignment tier in status draft.</summary>
    public static PendingAssignmentTier NewPendingAssignmentTier(string code, string title)
    {
        return new PendingAssignmentTier(code, title, 0, "", "", "", PendingAssignmentTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAssignmentTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAssignmentTierStatus next)
    {
        switch (Status)
        {
            case PendingAssignmentTierStatus.Draft:
                switch (next)
                {
                    case PendingAssignmentTierStatus.Active:
                    case PendingAssignmentTierStatus.Closed:
                        return true;
                }
                return false;
            case PendingAssignmentTierStatus.Pending:
                switch (next)
                {
                    case PendingAssignmentTierStatus.Active:
                        return true;
                }
                return false;
            case PendingAssignmentTierStatus.Active:
                return next == PendingAssignmentTierStatus.Suspended || next == PendingAssignmentTierStatus.Closed;
            case PendingAssignmentTierStatus.Suspended:
                return next == PendingAssignmentTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PendingAssignmentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending assignment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending assignment tier"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public PendingAssignmentTier Copy()
    {
        return new PendingAssignmentTier(Code, Title, Retries, OwnerId, Currency, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
