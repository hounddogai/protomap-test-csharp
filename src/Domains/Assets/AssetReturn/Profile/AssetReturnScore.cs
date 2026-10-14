namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return score is in its life cycle.</summary>
public enum AssetReturnScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A asset return score. Its code is unique within a tenant.</summary>
public sealed class AssetReturnScore(string code, string name, bool active, int attempts, string timezone, string skill, AssetReturnScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public int Attempts { get; set; } = attempts;
    public string Timezone { get; set; } = timezone;
    public string Skill { get; set; } = skill;
    public AssetReturnScoreStatus Status { get; set; } = status;

    /// <summary>Starts a asset return score in status created.</summary>
    public static AssetReturnScore NewAssetReturnScore(string code, string name)
    {
        return new AssetReturnScore(code, name, false, 0, "", "", AssetReturnScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnScoreStatus next)
    {
        switch (Status)
        {
            case AssetReturnScoreStatus.Created:
                return next == AssetReturnScoreStatus.Validated || next == AssetReturnScoreStatus.Retired;
            case AssetReturnScoreStatus.Validated:
                switch (next)
                {
                    case AssetReturnScoreStatus.Published:
                        return true;
                }
                return false;
            case AssetReturnScoreStatus.Published:
                switch (next)
                {
                    case AssetReturnScoreStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public AssetReturnScore Copy()
    {
        return new AssetReturnScore(Code, Name, Active, Attempts, Timezone, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
