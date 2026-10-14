namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global floor plan rollup is in its life cycle.</summary>
public enum GlobalFloorPlanRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A global floor plan rollup. Its code is unique within a tenant.</summary>
public sealed class GlobalFloorPlanRollup(string code, string label, string reviewer, double margin, int updatedAt, int tenureMonths, List<string> tags, string phone, string timezone, GlobalFloorPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public double Margin { get; set; } = margin;
    public int UpdatedAt { get; set; } = updatedAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public List<string> Tags { get; set; } = tags;
    public string Phone { get; set; } = phone;
    public string Timezone { get; set; } = timezone;
    public GlobalFloorPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a global floor plan rollup in status created.</summary>
    public static GlobalFloorPlanRollup NewGlobalFloorPlanRollup(string code, string label)
    {
        return new GlobalFloorPlanRollup(
            code,
            label,
            "",
            0.0,
            0,
            0,
            new List<string>(),
            "",
            "",
            GlobalFloorPlanRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalFloorPlanRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalFloorPlanRollupStatus next)
    {
        switch (Status)
        {
            case GlobalFloorPlanRollupStatus.Created:
                switch (next)
                {
                    case GlobalFloorPlanRollupStatus.Validated:
                        return true;
                }
                return false;
            case GlobalFloorPlanRollupStatus.Validated:
                switch (next)
                {
                    case GlobalFloorPlanRollupStatus.Published:
                        return true;
                }
                return false;
            case GlobalFloorPlanRollupStatus.Published:
                return next == GlobalFloorPlanRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GlobalFloorPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global floor plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedValue()
    {
        return Math.Round((double)TenureMonths * Margin, 2);
    }

    public GlobalFloorPlanRollup Copy()
    {
        return new GlobalFloorPlanRollup(
            Code,
            Label,
            Reviewer,
            Margin,
            UpdatedAt,
            TenureMonths,
            Tags,
            Phone,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
