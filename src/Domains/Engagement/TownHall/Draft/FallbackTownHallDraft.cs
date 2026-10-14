namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback town hall draft is in its life cycle.</summary>
public enum FallbackTownHallDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A fallback town hall draft. Its code is unique within a tenant.</summary>
public sealed class FallbackTownHallDraft(string code, string label, double hours, List<string> tags, string salaryBand, double fte, string checksum, string locale, double minimum, double cost, string segment, FallbackTownHallDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public List<string> Tags { get; set; } = tags;
    public string SalaryBand { get; set; } = salaryBand;
    public double Fte { get; set; } = fte;
    public string Checksum { get; set; } = checksum;
    public string Locale { get; set; } = locale;
    public double Minimum { get; set; } = minimum;
    public double Cost { get; set; } = cost;
    public string Segment { get; set; } = segment;
    public FallbackTownHallDraftStatus Status { get; set; } = status;

    /// <summary>Starts a fallback town hall draft in status requested.</summary>
    public static FallbackTownHallDraft NewFallbackTownHallDraft(string code, string label)
    {
        return new FallbackTownHallDraft(
            code,
            label,
            0.0,
            new List<string>(),
            "",
            0.0,
            "",
            "",
            0.0,
            0.0,
            "",
            FallbackTownHallDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackTownHallDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackTownHallDraftStatus next)
    {
        switch (Status)
        {
            case FallbackTownHallDraftStatus.Requested:
                return next == FallbackTownHallDraftStatus.Revoked || next == FallbackTownHallDraftStatus.Expired;
            case FallbackTownHallDraftStatus.Granted:
                return next == FallbackTownHallDraftStatus.Revoked || next == FallbackTownHallDraftStatus.Expired;
            case FallbackTownHallDraftStatus.Revoked:
                return next == FallbackTownHallDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FallbackTownHallDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback town hall draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback town hall draft"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetExposure()
    {
        return Math.Round(Cost + Hours, 2);
    }

    public FallbackTownHallDraft Copy()
    {
        return new FallbackTownHallDraft(
            Code,
            Label,
            Hours,
            Tags,
            SalaryBand,
            Fte,
            Checksum,
            Locale,
            Minimum,
            Cost,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
