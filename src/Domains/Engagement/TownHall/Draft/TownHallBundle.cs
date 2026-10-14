namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall bundle is in its life cycle.</summary>
public enum TownHallBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A town hall bundle as the town hall team keeps it.</summary>
public sealed class TownHallBundle(string code, string name, double score, string grade, string segment, bool pinned, double discount, bool approved, double ratio, string location, string locale, TownHallBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string Grade { get; set; } = grade;
    public string Segment { get; set; } = segment;
    public bool Pinned { get; set; } = pinned;
    public double Discount { get; set; } = discount;
    public bool Approved { get; set; } = approved;
    public double Ratio { get; set; } = ratio;
    public string Location { get; set; } = location;
    public string Locale { get; set; } = locale;
    public TownHallBundleStatus Status { get; set; } = status;

    /// <summary>Starts a town hall bundle in status new.</summary>
    public static TownHallBundle NewTownHallBundle(string code, string name)
    {
        return new TownHallBundle(code, name, 0.0, "", "", false, 0.0, false, 0.0, "", "", TownHallBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallBundleStatus next)
    {
        switch (Status)
        {
            case TownHallBundleStatus.New:
                return next == TownHallBundleStatus.Running || next == TownHallBundleStatus.Failed;
            case TownHallBundleStatus.Queued:
                switch (next)
                {
                    case TownHallBundleStatus.Running:
                    case TownHallBundleStatus.Failed:
                        return true;
                }
                return false;
            case TownHallBundleStatus.Running:
                switch (next)
                {
                    case TownHallBundleStatus.Done:
                        return true;
                }
                return false;
            case TownHallBundleStatus.Done:
                switch (next)
                {
                    case TownHallBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"town hall bundle"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Score - Ratio, 2);
    }

    public TownHallBundle Copy()
    {
        return new TownHallBundle(
            Code,
            Name,
            Score,
            Grade,
            Segment,
            Pinned,
            Discount,
            Approved,
            Ratio,
            Location,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
