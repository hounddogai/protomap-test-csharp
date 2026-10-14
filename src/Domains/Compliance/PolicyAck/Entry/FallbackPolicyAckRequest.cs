namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback policy ack request is in its life cycle.</summary>
public enum FallbackPolicyAckRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback policy ack request. Its code is unique within a tenant.</summary>
public sealed class FallbackPolicyAckRequest(string code, string label, double rating, int graceDays, string reviewer, double margin, double discount, string team, string source, int leaveDays, FallbackPolicyAckRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public int GraceDays { get; set; } = graceDays;
    public string Reviewer { get; set; } = reviewer;
    public double Margin { get; set; } = margin;
    public double Discount { get; set; } = discount;
    public string Team { get; set; } = team;
    public string Source { get; set; } = source;
    public int LeaveDays { get; set; } = leaveDays;
    public FallbackPolicyAckRequestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback policy ack request in status planned.</summary>
    public static FallbackPolicyAckRequest NewFallbackPolicyAckRequest(string code, string label)
    {
        return new FallbackPolicyAckRequest(
            code,
            label,
            0.0,
            0,
            "",
            0.0,
            0.0,
            "",
            "",
            0,
            FallbackPolicyAckRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPolicyAckRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPolicyAckRequestStatus next)
    {
        switch (Status)
        {
            case FallbackPolicyAckRequestStatus.Planned:
                return next == FallbackPolicyAckRequestStatus.Cancelled;
            case FallbackPolicyAckRequestStatus.Scheduled:
                return next == FallbackPolicyAckRequestStatus.InProgress || next == FallbackPolicyAckRequestStatus.Completed;
            case FallbackPolicyAckRequestStatus.InProgress:
                switch (next)
                {
                    case FallbackPolicyAckRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackPolicyAckRequestStatus.Completed:
                switch (next)
                {
                    case FallbackPolicyAckRequestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackPolicyAckRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback policy ack request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)GraceDays + Margin, 2);
    }

    public FallbackPolicyAckRequest Copy()
    {
        return new FallbackPolicyAckRequest(
            Code,
            Label,
            Rating,
            GraceDays,
            Reviewer,
            Margin,
            Discount,
            Team,
            Source,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
