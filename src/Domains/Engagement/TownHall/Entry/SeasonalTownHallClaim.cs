namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal town hall claim is in its life cycle.</summary>
public enum SeasonalTownHallClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A seasonal town hall claim as the town hall team keeps it.</summary>
public sealed class SeasonalTownHallClaim(string code, string label, string source, bool active, int attempts, double discount, SeasonalTownHallClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public bool Active { get; set; } = active;
    public int Attempts { get; set; } = attempts;
    public double Discount { get; set; } = discount;
    public SeasonalTownHallClaimStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal town hall claim in status new.</summary>
    public static SeasonalTownHallClaim NewSeasonalTownHallClaim(string code, string label)
    {
        return new SeasonalTownHallClaim(code, label, "", false, 0, 0.0, SeasonalTownHallClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTownHallClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTownHallClaimStatus next)
    {
        switch (Status)
        {
            case SeasonalTownHallClaimStatus.New:
                switch (next)
                {
                    case SeasonalTownHallClaimStatus.Queued:
                        return true;
                }
                return false;
            case SeasonalTownHallClaimStatus.Queued:
                return next == SeasonalTownHallClaimStatus.Failed;
            case SeasonalTownHallClaimStatus.Running:
                return next == SeasonalTownHallClaimStatus.Done || next == SeasonalTownHallClaimStatus.Failed;
            case SeasonalTownHallClaimStatus.Done:
                return next == SeasonalTownHallClaimStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SeasonalTownHallClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal town hall claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Attempts * Discount, 2);
    }

    public SeasonalTownHallClaim Copy()
    {
        return new SeasonalTownHallClaim(Code, Label, Source, Active, Attempts, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
