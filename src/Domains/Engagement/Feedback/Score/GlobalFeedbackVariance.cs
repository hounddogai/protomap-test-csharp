namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global feedback variance is in its life cycle.</summary>
public enum GlobalFeedbackVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A global feedback variance. Its code is unique within a tenant.</summary>
public sealed class GlobalFeedbackVariance(string code, string name, bool approved, double rate, string ownerId, string location, string externalRef, double floorValue, string locale, double hours, GlobalFeedbackVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public double Rate { get; set; } = rate;
    public string OwnerId { get; set; } = ownerId;
    public string Location { get; set; } = location;
    public string ExternalRef { get; set; } = externalRef;
    public double FloorValue { get; set; } = floorValue;
    public string Locale { get; set; } = locale;
    public double Hours { get; set; } = hours;
    public GlobalFeedbackVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a global feedback variance in status open.</summary>
    public static GlobalFeedbackVariance NewGlobalFeedbackVariance(string code, string name)
    {
        return new GlobalFeedbackVariance(
            code,
            name,
            false,
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            0.0,
            GlobalFeedbackVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalFeedbackVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalFeedbackVarianceStatus next)
    {
        switch (Status)
        {
            case GlobalFeedbackVarianceStatus.Open:
                switch (next)
                {
                    case GlobalFeedbackVarianceStatus.Archived:
                        return true;
                }
                return false;
            case GlobalFeedbackVarianceStatus.InReview:
                return next == GlobalFeedbackVarianceStatus.Resolved || next == GlobalFeedbackVarianceStatus.Archived;
            case GlobalFeedbackVarianceStatus.Resolved:
                return next == GlobalFeedbackVarianceStatus.Reopened || next == GlobalFeedbackVarianceStatus.Archived;
            case GlobalFeedbackVarianceStatus.Reopened:
                return next == GlobalFeedbackVarianceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalFeedbackVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global feedback variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round(FloorValue + Hours, 2);
    }

    public GlobalFeedbackVariance Copy()
    {
        return new GlobalFeedbackVariance(
            Code,
            Name,
            Approved,
            Rate,
            OwnerId,
            Location,
            ExternalRef,
            FloorValue,
            Locale,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
