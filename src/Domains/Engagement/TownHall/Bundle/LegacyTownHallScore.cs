namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy town hall score is in its life cycle.</summary>
public enum LegacyTownHallScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A legacy town hall score. Its code is unique within a tenant.</summary>
public sealed class LegacyTownHallScore(string code, string label, string externalRef, string name, string checksum, double maximum, string country, int attempts, LegacyTownHallScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public double Maximum { get; set; } = maximum;
    public string Country { get; set; } = country;
    public int Attempts { get; set; } = attempts;
    public LegacyTownHallScoreStatus Status { get; set; } = status;

    /// <summary>Starts a legacy town hall score in status requested.</summary>
    public static LegacyTownHallScore NewLegacyTownHallScore(string code, string label)
    {
        return new LegacyTownHallScore(code, label, "", "", "", 0.0, "", 0, LegacyTownHallScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTownHallScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTownHallScoreStatus next)
    {
        switch (Status)
        {
            case LegacyTownHallScoreStatus.Requested:
                return next == LegacyTownHallScoreStatus.Granted;
            case LegacyTownHallScoreStatus.Granted:
                return next == LegacyTownHallScoreStatus.Revoked || next == LegacyTownHallScoreStatus.Expired;
            case LegacyTownHallScoreStatus.Revoked:
                switch (next)
                {
                    case LegacyTownHallScoreStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTownHallScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy town hall score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Attempts + Maximum, 2);
    }

    public LegacyTownHallScore Copy()
    {
        return new LegacyTownHallScore(Code, Label, ExternalRef, Name, Checksum, Maximum, Country, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
