namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback ledger is in its life cycle.</summary>
public enum FeedbackLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One feedback ledger, with what the planners need to act on it.</summary>
public sealed class FeedbackLedger(string code, string label, string skill, int revision, string grade, string timezone, int leaveDays, double rating, FeedbackLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int Revision { get; set; } = revision;
    public string Grade { get; set; } = grade;
    public string Timezone { get; set; } = timezone;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rating { get; set; } = rating;
    public FeedbackLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a feedback ledger in status created.</summary>
    public static FeedbackLedger NewFeedbackLedger(string code, string label)
    {
        return new FeedbackLedger(code, label, "", 0, "", "", 0, 0.0, FeedbackLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackLedgerStatus next)
    {
        switch (Status)
        {
            case FeedbackLedgerStatus.Created:
                return next == FeedbackLedgerStatus.Validated;
            case FeedbackLedgerStatus.Validated:
                return next == FeedbackLedgerStatus.Retired;
            case FeedbackLedgerStatus.Published:
                switch (next)
                {
                    case FeedbackLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Rating - (double)Revision, 2);
    }

    public FeedbackLedger Copy()
    {
        return new FeedbackLedger(Code, Label, Skill, Revision, Grade, Timezone, LeaveDays, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
