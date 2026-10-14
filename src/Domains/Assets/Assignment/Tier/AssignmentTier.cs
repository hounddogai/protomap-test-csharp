namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment tier is in its life cycle.</summary>
public enum AssignmentTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A assignment tier as the assignment team keeps it.</summary>
public sealed class AssignmentTier(string code, string label, double rate, string summary, double weight, double fte, string region, AssignmentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Summary { get; set; } = summary;
    public double Weight { get; set; } = weight;
    public double Fte { get; set; } = fte;
    public string Region { get; set; } = region;
    public AssignmentTierStatus Status { get; set; } = status;

    /// <summary>Starts a assignment tier in status draft.</summary>
    public static AssignmentTier NewAssignmentTier(string code, string label)
    {
        return new AssignmentTier(code, label, 0.0, "", 0.0, 0.0, "", AssignmentTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentTierStatus next)
    {
        switch (Status)
        {
            case AssignmentTierStatus.Draft:
                return next == AssignmentTierStatus.Active || next == AssignmentTierStatus.Closed;
            case AssignmentTierStatus.Pending:
                switch (next)
                {
                    case AssignmentTierStatus.Suspended:
                    case AssignmentTierStatus.Closed:
                        return true;
                }
                return false;
            case AssignmentTierStatus.Active:
                return next == AssignmentTierStatus.Suspended || next == AssignmentTierStatus.Closed;
            case AssignmentTierStatus.Suspended:
                return next == AssignmentTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AssignmentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rate - Fte, 2);
    }

    public AssignmentTier Copy()
    {
        return new AssignmentTier(Code, Label, Rate, Summary, Weight, Fte, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
