namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy recognition baseline is in its life cycle.</summary>
public enum LegacyRecognitionBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a legacy recognition baseline and its current status.</summary>
public sealed class LegacyRecognitionBaseline(string code, string name, int priority, double weight, string team, string unit, string skill, string checksum, int tenureMonths, string ownerId, string timezone, LegacyRecognitionBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public double Weight { get; set; } = weight;
    public string Team { get; set; } = team;
    public string Unit { get; set; } = unit;
    public string Skill { get; set; } = skill;
    public string Checksum { get; set; } = checksum;
    public int TenureMonths { get; set; } = tenureMonths;
    public string OwnerId { get; set; } = ownerId;
    public string Timezone { get; set; } = timezone;
    public LegacyRecognitionBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a legacy recognition baseline in status draft.</summary>
    public static LegacyRecognitionBaseline NewLegacyRecognitionBaseline(string code, string name)
    {
        return new LegacyRecognitionBaseline(
            code,
            name,
            0,
            0.0,
            "",
            "",
            "",
            "",
            0,
            "",
            "",
            LegacyRecognitionBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRecognitionBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRecognitionBaselineStatus next)
    {
        switch (Status)
        {
            case LegacyRecognitionBaselineStatus.Draft:
                return next == LegacyRecognitionBaselineStatus.Active;
            case LegacyRecognitionBaselineStatus.Pending:
                switch (next)
                {
                    case LegacyRecognitionBaselineStatus.Active:
                        return true;
                }
                return false;
            case LegacyRecognitionBaselineStatus.Active:
                switch (next)
                {
                    case LegacyRecognitionBaselineStatus.Suspended:
                    case LegacyRecognitionBaselineStatus.Closed:
                        return true;
                }
                return false;
            case LegacyRecognitionBaselineStatus.Suspended:
                switch (next)
                {
                    case LegacyRecognitionBaselineStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyRecognitionBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy recognition baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy recognition baseline"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Weight + (double)TenureMonths, 2);
    }

    public LegacyRecognitionBaseline Copy()
    {
        return new LegacyRecognitionBaseline(
            Code,
            Name,
            Priority,
            Weight,
            Team,
            Unit,
            Skill,
            Checksum,
            TenureMonths,
            OwnerId,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
