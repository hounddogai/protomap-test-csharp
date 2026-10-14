namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global asset return profile is in its life cycle.</summary>
public enum GlobalAssetReturnProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A global asset return profile. Its code is unique within a tenant.</summary>
public sealed class GlobalAssetReturnProfile(string code, string label, int createdAt, int quantity, string team, string timezone, GlobalAssetReturnProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public int Quantity { get; set; } = quantity;
    public string Team { get; set; } = team;
    public string Timezone { get; set; } = timezone;
    public GlobalAssetReturnProfileStatus Status { get; set; } = status;

    /// <summary>Starts a global asset return profile in status open.</summary>
    public static GlobalAssetReturnProfile NewGlobalAssetReturnProfile(string code, string label)
    {
        return new GlobalAssetReturnProfile(code, label, 0, 0, "", "", GlobalAssetReturnProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAssetReturnProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAssetReturnProfileStatus next)
    {
        switch (Status)
        {
            case GlobalAssetReturnProfileStatus.Open:
                switch (next)
                {
                    case GlobalAssetReturnProfileStatus.Archived:
                        return true;
                }
                return false;
            case GlobalAssetReturnProfileStatus.InReview:
                return next == GlobalAssetReturnProfileStatus.Resolved;
            case GlobalAssetReturnProfileStatus.Resolved:
                return next == GlobalAssetReturnProfileStatus.Reopened || next == GlobalAssetReturnProfileStatus.Archived;
            case GlobalAssetReturnProfileStatus.Reopened:
                return next == GlobalAssetReturnProfileStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalAssetReturnProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global asset return profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)CreatedAt * (double)Quantity, 2);
    }

    public GlobalAssetReturnProfile Copy()
    {
        return new GlobalAssetReturnProfile(Code, Label, CreatedAt, Quantity, Team, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
