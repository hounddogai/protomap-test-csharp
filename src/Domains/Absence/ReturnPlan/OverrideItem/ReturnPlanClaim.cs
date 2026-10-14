namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan claim is in its life cycle.</summary>
public enum ReturnPlanClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A return plan claim. Its code is unique within a tenant.</summary>
public sealed class ReturnPlanClaim(string code, string title, Dictionary<string, string> attributes, double weight, int leaveDays, string checksum, string segment, string region, bool archived, ReturnPlanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Weight { get; set; } = weight;
    public int LeaveDays { get; set; } = leaveDays;
    public string Checksum { get; set; } = checksum;
    public string Segment { get; set; } = segment;
    public string Region { get; set; } = region;
    public bool Archived { get; set; } = archived;
    public ReturnPlanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a return plan claim in status open.</summary>
    public static ReturnPlanClaim NewReturnPlanClaim(string code, string title)
    {
        return new ReturnPlanClaim(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            "",
            "",
            false,
            ReturnPlanClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanClaimStatus next)
    {
        switch (Status)
        {
            case ReturnPlanClaimStatus.Open:
                return next == ReturnPlanClaimStatus.Resolved;
            case ReturnPlanClaimStatus.InReview:
                return next == ReturnPlanClaimStatus.Reopened || next == ReturnPlanClaimStatus.Archived;
            case ReturnPlanClaimStatus.Resolved:
                return next == ReturnPlanClaimStatus.Reopened || next == ReturnPlanClaimStatus.Archived;
            case ReturnPlanClaimStatus.Reopened:
                switch (next)
                {
                    case ReturnPlanClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double EffectiveValue()
    {
        return Math.Round(Weight - (double)LeaveDays, 2);
    }

    public ReturnPlanClaim Copy()
    {
        return new ReturnPlanClaim(
            Code,
            Title,
            Attributes,
            Weight,
            LeaveDays,
            Checksum,
            Segment,
            Region,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
