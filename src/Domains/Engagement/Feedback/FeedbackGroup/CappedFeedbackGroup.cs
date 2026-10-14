namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped feedback group is in its life cycle.</summary>
public enum CappedFeedbackGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped feedback group. Its code is unique within a tenant.</summary>
public sealed class CappedFeedbackGroup(string code, string label, bool pinned, string description, string managerId, string externalRef, double budget, CappedFeedbackGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Description { get; set; } = description;
    public string ManagerId { get; set; } = managerId;
    public string ExternalRef { get; set; } = externalRef;
    public double Budget { get; set; } = budget;
    public CappedFeedbackGroupStatus Status { get; set; } = status;

    /// <summary>Starts a capped feedback group in status requested.</summary>
    public static CappedFeedbackGroup NewCappedFeedbackGroup(string code, string label)
    {
        return new CappedFeedbackGroup(code, label, false, "", "", "", 0.0, CappedFeedbackGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedFeedbackGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedFeedbackGroupStatus next)
    {
        switch (Status)
        {
            case CappedFeedbackGroupStatus.Requested:
                return next == CappedFeedbackGroupStatus.Revoked;
            case CappedFeedbackGroupStatus.Granted:
                return next == CappedFeedbackGroupStatus.Revoked;
            case CappedFeedbackGroupStatus.Revoked:
                return next == CappedFeedbackGroupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedFeedbackGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped feedback group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped feedback group"} {Code}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public CappedFeedbackGroup Copy()
    {
        return new CappedFeedbackGroup(Code, Label, Pinned, Description, ManagerId, ExternalRef, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
