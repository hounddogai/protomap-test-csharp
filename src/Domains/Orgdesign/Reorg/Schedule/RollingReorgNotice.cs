namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling reorg notice is in its life cycle.</summary>
public enum RollingReorgNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling reorg notice as the reorg team keeps it.</summary>
public sealed class RollingReorgNotice(string code, string label, string phone, string name, bool pinned, double rating, double margin, string region, RollingReorgNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public double Rating { get; set; } = rating;
    public double Margin { get; set; } = margin;
    public string Region { get; set; } = region;
    public RollingReorgNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a rolling reorg notice in status open.</summary>
    public static RollingReorgNotice NewRollingReorgNotice(string code, string label)
    {
        return new RollingReorgNotice(code, label, "", "", false, 0.0, 0.0, "", RollingReorgNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingReorgNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingReorgNoticeStatus next)
    {
        switch (Status)
        {
            case RollingReorgNoticeStatus.Open:
                switch (next)
                {
                    case RollingReorgNoticeStatus.Reopened:
                    case RollingReorgNoticeStatus.Archived:
                        return true;
                }
                return false;
            case RollingReorgNoticeStatus.InReview:
                switch (next)
                {
                    case RollingReorgNoticeStatus.Resolved:
                    case RollingReorgNoticeStatus.Archived:
                        return true;
                }
                return false;
            case RollingReorgNoticeStatus.Resolved:
                return next == RollingReorgNoticeStatus.Reopened;
            case RollingReorgNoticeStatus.Reopened:
                switch (next)
                {
                    case RollingReorgNoticeStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingReorgNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling reorg notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling reorg notice"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Margin - Rating, 2);
    }

    public RollingReorgNotice Copy()
    {
        return new RollingReorgNotice(Code, Label, Phone, Name, Pinned, Rating, Margin, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
