namespace Acme.Workforce.Engagement.TownHall.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall tier is in its life cycle.</summary>
public enum TownHallTierStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A town hall tier. Its code is unique within a tenant.</summary>
public sealed class TownHallTier(string code, string label, double margin, Dictionary<string, string> attributes, int endsAt, int capacity, string team, TownHallTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int EndsAt { get; set; } = endsAt;
    public int Capacity { get; set; } = capacity;
    public string Team { get; set; } = team;
    public TownHallTierStatus Status { get; set; } = status;

    /// <summary>Starts a town hall tier in status requested.</summary>
    public static TownHallTier NewTownHallTier(string code, string label)
    {
        return new TownHallTier(
            code,
            label,
            0.0,
            new Dictionary<string, string>(),
            0,
            0,
            "",
            TownHallTierStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallTierStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallTierStatus next)
    {
        switch (Status)
        {
            case TownHallTierStatus.Requested:
                return next == TownHallTierStatus.Granted;
            case TownHallTierStatus.Granted:
                return next == TownHallTierStatus.Revoked || next == TownHallTierStatus.Expired;
            case TownHallTierStatus.Revoked:
                return next == TownHallTierStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TownHallTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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
        return Math.Round((double)EndsAt - (double)Capacity, 2);
    }

    public TownHallTier Copy()
    {
        return new TownHallTier(Code, Label, Margin, Attributes, EndsAt, Capacity, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
