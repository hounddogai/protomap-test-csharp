namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy cost model group is in its life cycle.</summary>
public enum LegacyCostModelGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One legacy cost model group, with what the planners need to act on it.</summary>
public sealed class LegacyCostModelGroup(string code, string label, string location, string title, string badge, int expiresAt, double floorValue, string contactEmail, string team, string reviewer, string name, LegacyCostModelGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public int ExpiresAt { get; set; } = expiresAt;
    public double FloorValue { get; set; } = floorValue;
    public string ContactEmail { get; set; } = contactEmail;
    public string Team { get; set; } = team;
    public string Reviewer { get; set; } = reviewer;
    public string Name { get; set; } = name;
    public LegacyCostModelGroupStatus Status { get; set; } = status;

    /// <summary>Starts a legacy cost model group in status created.</summary>
    public static LegacyCostModelGroup NewLegacyCostModelGroup(string code, string label)
    {
        return new LegacyCostModelGroup(
            code,
            label,
            "",
            "",
            "",
            0,
            0.0,
            "",
            "",
            "",
            "",
            LegacyCostModelGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyCostModelGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyCostModelGroupStatus next)
    {
        switch (Status)
        {
            case LegacyCostModelGroupStatus.Created:
                return next == LegacyCostModelGroupStatus.Published || next == LegacyCostModelGroupStatus.Retired;
            case LegacyCostModelGroupStatus.Validated:
                switch (next)
                {
                    case LegacyCostModelGroupStatus.Published:
                        return true;
                }
                return false;
            case LegacyCostModelGroupStatus.Published:
                switch (next)
                {
                    case LegacyCostModelGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyCostModelGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy cost model group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(FloorValue * (double)ExpiresAt, 2);
    }

    public LegacyCostModelGroup Copy()
    {
        return new LegacyCostModelGroup(
            Code,
            Label,
            Location,
            Title,
            Badge,
            ExpiresAt,
            FloorValue,
            ContactEmail,
            Team,
            Reviewer,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
