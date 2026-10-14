namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination profile is in its life cycle.</summary>
public enum NominationProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One nomination profile, with what the planners need to act on it.</summary>
public sealed class NominationProfile(string code, string title, double minimum, string label, double budget, string reviewer, double amount, NominationProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Reviewer { get; set; } = reviewer;
    public double Amount { get; set; } = amount;
    public NominationProfileStatus Status { get; set; } = status;

    /// <summary>Starts a nomination profile in status open.</summary>
    public static NominationProfile NewNominationProfile(string code, string title)
    {
        return new NominationProfile(code, title, 0.0, "", 0.0, "", 0.0, NominationProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationProfileStatus next)
    {
        switch (Status)
        {
            case NominationProfileStatus.Open:
                return next == NominationProfileStatus.Reopened || next == NominationProfileStatus.Archived;
            case NominationProfileStatus.InReview:
                return next == NominationProfileStatus.Resolved || next == NominationProfileStatus.Reopened;
            case NominationProfileStatus.Resolved:
                switch (next)
                {
                    case NominationProfileStatus.Reopened:
                        return true;
                }
                return false;
            case NominationProfileStatus.Reopened:
                switch (next)
                {
                    case NominationProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Budget - Amount, 2);
    }

    public NominationProfile Copy()
    {
        return new NominationProfile(Code, Title, Minimum, Label, Budget, Reviewer, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
