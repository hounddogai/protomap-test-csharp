namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending policy ack group is in its life cycle.</summary>
public enum PendingPolicyAckGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a pending policy ack group and its current status.</summary>
public sealed class PendingPolicyAckGroup(string code, string title, double threshold, double budget, int updatedAt, double amount, PendingPolicyAckGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public double Budget { get; set; } = budget;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Amount { get; set; } = amount;
    public PendingPolicyAckGroupStatus Status { get; set; } = status;

    /// <summary>Starts a pending policy ack group in status new.</summary>
    public static PendingPolicyAckGroup NewPendingPolicyAckGroup(string code, string title)
    {
        return new PendingPolicyAckGroup(code, title, 0.0, 0.0, 0, 0.0, PendingPolicyAckGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPolicyAckGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPolicyAckGroupStatus next)
    {
        switch (Status)
        {
            case PendingPolicyAckGroupStatus.New:
                switch (next)
                {
                    case PendingPolicyAckGroupStatus.Queued:
                    case PendingPolicyAckGroupStatus.Done:
                        return true;
                }
                return false;
            case PendingPolicyAckGroupStatus.Queued:
                switch (next)
                {
                    case PendingPolicyAckGroupStatus.Running:
                        return true;
                }
                return false;
            case PendingPolicyAckGroupStatus.Running:
                switch (next)
                {
                    case PendingPolicyAckGroupStatus.Done:
                        return true;
                }
                return false;
            case PendingPolicyAckGroupStatus.Done:
                return next == PendingPolicyAckGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingPolicyAckGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending policy ack group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round(Threshold + (double)UpdatedAt, 2);
    }

    public PendingPolicyAckGroup Copy()
    {
        return new PendingPolicyAckGroup(Code, Title, Threshold, Budget, UpdatedAt, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
