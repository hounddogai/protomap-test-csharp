namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge override is in its life cycle.</summary>
public enum AccessBadgeOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One access badge override, with what the planners need to act on it.</summary>
public sealed class AccessBadgeOverride(string code, string label, string title, double score, string reviewer, string contactEmail, string location, AccessBadgeOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Reviewer { get; set; } = reviewer;
    public string ContactEmail { get; set; } = contactEmail;
    public string Location { get; set; } = location;
    public AccessBadgeOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a access badge override in status requested.</summary>
    public static AccessBadgeOverride NewAccessBadgeOverride(string code, string label)
    {
        return new AccessBadgeOverride(code, label, "", 0.0, "", "", "", AccessBadgeOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeOverrideStatus next)
    {
        switch (Status)
        {
            case AccessBadgeOverrideStatus.Requested:
                return next == AccessBadgeOverrideStatus.Expired;
            case AccessBadgeOverrideStatus.Granted:
                switch (next)
                {
                    case AccessBadgeOverrideStatus.Revoked:
                    case AccessBadgeOverrideStatus.Expired:
                        return true;
                }
                return false;
            case AccessBadgeOverrideStatus.Revoked:
                switch (next)
                {
                    case AccessBadgeOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"access badge override"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AccessBadgeOverride Copy()
    {
        return new AccessBadgeOverride(Code, Label, Title, Score, Reviewer, ContactEmail, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
