namespace Acme.Workforce.Engagement.TownHall.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall claim is in its life cycle.</summary>
public enum TownHallClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One town hall claim, with what the planners need to act on it.</summary>
public sealed class TownHallClaim(string code, string title, string ownerId, string summary, string notes, double ratio, double rate, string checksum, string name, TownHallClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Summary { get; set; } = summary;
    public string Notes { get; set; } = notes;
    public double Ratio { get; set; } = ratio;
    public double Rate { get; set; } = rate;
    public string Checksum { get; set; } = checksum;
    public string Name { get; set; } = name;
    public TownHallClaimStatus Status { get; set; } = status;

    /// <summary>Starts a town hall claim in status idle.</summary>
    public static TownHallClaim NewTownHallClaim(string code, string title)
    {
        return new TownHallClaim(code, title, "", "", "", 0.0, 0.0, "", "", TownHallClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallClaimStatus next)
    {
        switch (Status)
        {
            case TownHallClaimStatus.Idle:
                return next == TownHallClaimStatus.Ready || next == TownHallClaimStatus.Stopped;
            case TownHallClaimStatus.Warming:
                switch (next)
                {
                    case TownHallClaimStatus.Ready:
                    case TownHallClaimStatus.Stopped:
                        return true;
                }
                return false;
            case TownHallClaimStatus.Ready:
                return next == TownHallClaimStatus.Draining || next == TownHallClaimStatus.Stopped;
            case TownHallClaimStatus.Draining:
                return next == TownHallClaimStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TownHallClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rate - Ratio, 2);
    }

    public TownHallClaim Copy()
    {
        return new TownHallClaim(Code, Title, OwnerId, Summary, Notes, Ratio, Rate, Checksum, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
