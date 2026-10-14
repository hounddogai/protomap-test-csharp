namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy career path plan is in its life cycle.</summary>
public enum LegacyCareerPathPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A legacy career path plan as the career path team keeps it.</summary>
public sealed class LegacyCareerPathPlan(string code, string name, bool visible, double weight, int version, string label, LegacyCareerPathPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public double Weight { get; set; } = weight;
    public int Version { get; set; } = version;
    public string Label { get; set; } = label;
    public LegacyCareerPathPlanStatus Status { get; set; } = status;

    /// <summary>Starts a legacy career path plan in status created.</summary>
    public static LegacyCareerPathPlan NewLegacyCareerPathPlan(string code, string name)
    {
        return new LegacyCareerPathPlan(code, name, false, 0.0, 0, "", LegacyCareerPathPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyCareerPathPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyCareerPathPlanStatus next)
    {
        switch (Status)
        {
            case LegacyCareerPathPlanStatus.Created:
                return next == LegacyCareerPathPlanStatus.Validated || next == LegacyCareerPathPlanStatus.Retired;
            case LegacyCareerPathPlanStatus.Validated:
                switch (next)
                {
                    case LegacyCareerPathPlanStatus.Published:
                    case LegacyCareerPathPlanStatus.Retired:
                        return true;
                }
                return false;
            case LegacyCareerPathPlanStatus.Published:
                return next == LegacyCareerPathPlanStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LegacyCareerPathPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy career path plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Weight - (double)Version, 2);
    }

    public LegacyCareerPathPlan Copy()
    {
        return new LegacyCareerPathPlan(Code, Name, Visible, Weight, Version, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
