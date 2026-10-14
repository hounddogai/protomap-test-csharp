namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered access badge bundle is in its life cycle.</summary>
public enum TieredAccessBadgeBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered access badge bundle. Its code is unique within a tenant.</summary>
public sealed class TieredAccessBadgeBundle(string code, string title, string category, bool active, int endsAt, string jobCode, string team, bool visible, int startsAt, TieredAccessBadgeBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public int EndsAt { get; set; } = endsAt;
    public string JobCode { get; set; } = jobCode;
    public string Team { get; set; } = team;
    public bool Visible { get; set; } = visible;
    public int StartsAt { get; set; } = startsAt;
    public TieredAccessBadgeBundleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered access badge bundle in status planned.</summary>
    public static TieredAccessBadgeBundle NewTieredAccessBadgeBundle(string code, string title)
    {
        return new TieredAccessBadgeBundle(
            code,
            title,
            "",
            false,
            0,
            "",
            "",
            false,
            0,
            TieredAccessBadgeBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAccessBadgeBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAccessBadgeBundleStatus next)
    {
        switch (Status)
        {
            case TieredAccessBadgeBundleStatus.Planned:
                switch (next)
                {
                    case TieredAccessBadgeBundleStatus.Completed:
                    case TieredAccessBadgeBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredAccessBadgeBundleStatus.Scheduled:
                return next == TieredAccessBadgeBundleStatus.InProgress || next == TieredAccessBadgeBundleStatus.Completed;
            case TieredAccessBadgeBundleStatus.InProgress:
                switch (next)
                {
                    case TieredAccessBadgeBundleStatus.Completed:
                    case TieredAccessBadgeBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredAccessBadgeBundleStatus.Completed:
                return next == TieredAccessBadgeBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredAccessBadgeBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered access badge bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered access badge bundle"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)EndsAt + (double)StartsAt, 2);
    }

    public TieredAccessBadgeBundle Copy()
    {
        return new TieredAccessBadgeBundle(
            Code,
            Title,
            Category,
            Active,
            EndsAt,
            JobCode,
            Team,
            Visible,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
