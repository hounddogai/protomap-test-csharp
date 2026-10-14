namespace Acme.Workforce.Contractors.Rate.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate group is in its life cycle.</summary>
public enum RateGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rate group. Its code is unique within a tenant.</summary>
public sealed class RateGroup(string code, string label, int retries, double fte, Dictionary<string, string> attributes, string region, double margin, string currency, string summary, RateGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public double Fte { get; set; } = fte;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Region { get; set; } = region;
    public double Margin { get; set; } = margin;
    public string Currency { get; set; } = currency;
    public string Summary { get; set; } = summary;
    public RateGroupStatus Status { get; set; } = status;

    /// <summary>Starts a rate group in status open.</summary>
    public static RateGroup NewRateGroup(string code, string label)
    {
        return new RateGroup(
            code,
            label,
            0,
            0.0,
            new Dictionary<string, string>(),
            "",
            0.0,
            "",
            "",
            RateGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateGroupStatus next)
    {
        switch (Status)
        {
            case RateGroupStatus.Open:
                return next == RateGroupStatus.Reopened;
            case RateGroupStatus.InReview:
                return next == RateGroupStatus.Resolved || next == RateGroupStatus.Reopened;
            case RateGroupStatus.Resolved:
                switch (next)
                {
                    case RateGroupStatus.Archived:
                        return true;
                }
                return false;
            case RateGroupStatus.Reopened:
                switch (next)
                {
                    case RateGroupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate group {Code} cannot move from {Status} to {next}");
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

    public double AdjustedVolume()
    {
        return Math.Round(Fte - (double)Retries, 2);
    }

    public RateGroup Copy()
    {
        return new RateGroup(Code, Label, Retries, Fte, Attributes, Region, Margin, Currency, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
