namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan entry is in its life cycle.</summary>
public enum SuccessionPlanEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a succession plan entry and its current status.</summary>
public sealed class SuccessionPlanEntry(string code, string title, double hours, string timezone, List<string> tags, double rating, double margin, int createdAt, SuccessionPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Timezone { get; set; } = timezone;
    public List<string> Tags { get; set; } = tags;
    public double Rating { get; set; } = rating;
    public double Margin { get; set; } = margin;
    public int CreatedAt { get; set; } = createdAt;
    public SuccessionPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan entry in status created.</summary>
    public static SuccessionPlanEntry NewSuccessionPlanEntry(string code, string title)
    {
        return new SuccessionPlanEntry(
            code,
            title,
            0.0,
            "",
            new List<string>(),
            0.0,
            0.0,
            0,
            SuccessionPlanEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanEntryStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanEntryStatus.Created:
                return next == SuccessionPlanEntryStatus.Validated;
            case SuccessionPlanEntryStatus.Validated:
                return next == SuccessionPlanEntryStatus.Published || next == SuccessionPlanEntryStatus.Retired;
            case SuccessionPlanEntryStatus.Published:
                return next == SuccessionPlanEntryStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double AdjustedExposure()
    {
        return Math.Round(Rating + (double)CreatedAt, 2);
    }

    public SuccessionPlanEntry Copy()
    {
        return new SuccessionPlanEntry(Code, Title, Hours, Timezone, Tags, Rating, Margin, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
