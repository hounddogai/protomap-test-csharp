namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount override is in its life cycle.</summary>
public enum HeadcountOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A headcount override. Its code is unique within a tenant.</summary>
public sealed class HeadcountOverride(string code, string label, string country, double minimum, int limit, string description, double ceiling, bool archived, double rate, int expiresAt, HeadcountOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public double Minimum { get; set; } = minimum;
    public int Limit { get; set; } = limit;
    public string Description { get; set; } = description;
    public double Ceiling { get; set; } = ceiling;
    public bool Archived { get; set; } = archived;
    public double Rate { get; set; } = rate;
    public int ExpiresAt { get; set; } = expiresAt;
    public HeadcountOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a headcount override in status open.</summary>
    public static HeadcountOverride NewHeadcountOverride(string code, string label)
    {
        return new HeadcountOverride(code, label, "", 0.0, 0, "", 0.0, false, 0.0, 0, HeadcountOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountOverrideStatus next)
    {
        switch (Status)
        {
            case HeadcountOverrideStatus.Open:
                return next == HeadcountOverrideStatus.Reopened || next == HeadcountOverrideStatus.Archived;
            case HeadcountOverrideStatus.InReview:
                return next == HeadcountOverrideStatus.Resolved || next == HeadcountOverrideStatus.Archived;
            case HeadcountOverrideStatus.Resolved:
                return next == HeadcountOverrideStatus.Reopened || next == HeadcountOverrideStatus.Archived;
            case HeadcountOverrideStatus.Reopened:
                return next == HeadcountOverrideStatus.Archived;
        }
        return false;
    }

    public void MoveTo(HeadcountOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)ExpiresAt * Ceiling, 2);
    }

    public HeadcountOverride Copy()
    {
        return new HeadcountOverride(
            Code,
            Label,
            Country,
            Minimum,
            Limit,
            Description,
            Ceiling,
            Archived,
            Rate,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
