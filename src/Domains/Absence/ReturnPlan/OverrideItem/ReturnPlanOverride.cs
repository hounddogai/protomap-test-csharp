namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan override is in its life cycle.</summary>
public enum ReturnPlanOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One return plan override, with what the planners need to act on it.</summary>
public sealed class ReturnPlanOverride(string code, string label, bool archived, string locale, string ownerId, int sequence, string reviewer, string country, Dictionary<string, string> attributes, string badge, double score, ReturnPlanOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Locale { get; set; } = locale;
    public string OwnerId { get; set; } = ownerId;
    public int Sequence { get; set; } = sequence;
    public string Reviewer { get; set; } = reviewer;
    public string Country { get; set; } = country;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Badge { get; set; } = badge;
    public double Score { get; set; } = score;
    public ReturnPlanOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a return plan override in status proposed.</summary>
    public static ReturnPlanOverride NewReturnPlanOverride(string code, string label)
    {
        return new ReturnPlanOverride(
            code,
            label,
            false,
            "",
            "",
            0,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            ReturnPlanOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanOverrideStatus next)
    {
        switch (Status)
        {
            case ReturnPlanOverrideStatus.Proposed:
                return next == ReturnPlanOverrideStatus.Rejected || next == ReturnPlanOverrideStatus.Withdrawn;
            case ReturnPlanOverrideStatus.Approved:
                switch (next)
                {
                    case ReturnPlanOverrideStatus.Rejected:
                        return true;
                }
                return false;
            case ReturnPlanOverrideStatus.Rejected:
                switch (next)
                {
                    case ReturnPlanOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Sequence * Score, 2);
    }

    public ReturnPlanOverride Copy()
    {
        return new ReturnPlanOverride(
            Code,
            Label,
            Archived,
            Locale,
            OwnerId,
            Sequence,
            Reviewer,
            Country,
            Attributes,
            Badge,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
