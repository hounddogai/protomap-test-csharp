namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary policy ack override is in its life cycle.</summary>
public enum PrimaryPolicyAckOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary policy ack override as the policy ack team keeps it.</summary>
public sealed class PrimaryPolicyAckOverride(string code, string title, int graceDays, bool pinned, string notes, double hours, PrimaryPolicyAckOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public bool Pinned { get; set; } = pinned;
    public string Notes { get; set; } = notes;
    public double Hours { get; set; } = hours;
    public PrimaryPolicyAckOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a primary policy ack override in status open.</summary>
    public static PrimaryPolicyAckOverride NewPrimaryPolicyAckOverride(string code, string title)
    {
        return new PrimaryPolicyAckOverride(code, title, 0, false, "", 0.0, PrimaryPolicyAckOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPolicyAckOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPolicyAckOverrideStatus next)
    {
        switch (Status)
        {
            case PrimaryPolicyAckOverrideStatus.Open:
                return next == PrimaryPolicyAckOverrideStatus.Resolved;
            case PrimaryPolicyAckOverrideStatus.InReview:
                switch (next)
                {
                    case PrimaryPolicyAckOverrideStatus.Reopened:
                    case PrimaryPolicyAckOverrideStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryPolicyAckOverrideStatus.Resolved:
                return next == PrimaryPolicyAckOverrideStatus.Reopened || next == PrimaryPolicyAckOverrideStatus.Archived;
            case PrimaryPolicyAckOverrideStatus.Reopened:
                switch (next)
                {
                    case PrimaryPolicyAckOverrideStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryPolicyAckOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary policy ack override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Hours - (double)GraceDays, 2);
    }

    public PrimaryPolicyAckOverride Copy()
    {
        return new PrimaryPolicyAckOverride(Code, Title, GraceDays, Pinned, Notes, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
