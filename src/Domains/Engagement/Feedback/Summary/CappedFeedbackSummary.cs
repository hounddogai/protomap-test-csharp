namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped feedback summary is in its life cycle.</summary>
public enum CappedFeedbackSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a capped feedback summary and its current status.</summary>
public sealed class CappedFeedbackSummary(string code, string label, double cost, string description, string contactEmail, string ownerId, CappedFeedbackSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public string Description { get; set; } = description;
    public string ContactEmail { get; set; } = contactEmail;
    public string OwnerId { get; set; } = ownerId;
    public CappedFeedbackSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a capped feedback summary in status proposed.</summary>
    public static CappedFeedbackSummary NewCappedFeedbackSummary(string code, string label)
    {
        return new CappedFeedbackSummary(code, label, 0.0, "", "", "", CappedFeedbackSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedFeedbackSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedFeedbackSummaryStatus next)
    {
        switch (Status)
        {
            case CappedFeedbackSummaryStatus.Proposed:
                switch (next)
                {
                    case CappedFeedbackSummaryStatus.Rejected:
                    case CappedFeedbackSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedFeedbackSummaryStatus.Approved:
                return next == CappedFeedbackSummaryStatus.Rejected;
            case CappedFeedbackSummaryStatus.Rejected:
                switch (next)
                {
                    case CappedFeedbackSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedFeedbackSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped feedback summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped feedback summary"} {Code}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public CappedFeedbackSummary Copy()
    {
        return new CappedFeedbackSummary(Code, Label, Cost, Description, ContactEmail, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
