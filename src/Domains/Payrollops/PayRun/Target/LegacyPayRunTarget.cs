namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay run target is in its life cycle.</summary>
public enum LegacyPayRunTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A legacy pay run target as the pay run team keeps it.</summary>
public sealed class LegacyPayRunTarget(string code, string name, string badge, double ratio, string unit, string checksum, string contactEmail, string skill, LegacyPayRunTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public double Ratio { get; set; } = ratio;
    public string Unit { get; set; } = unit;
    public string Checksum { get; set; } = checksum;
    public string ContactEmail { get; set; } = contactEmail;
    public string Skill { get; set; } = skill;
    public LegacyPayRunTargetStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay run target in status created.</summary>
    public static LegacyPayRunTarget NewLegacyPayRunTarget(string code, string name)
    {
        return new LegacyPayRunTarget(code, name, "", 0.0, "", "", "", "", LegacyPayRunTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayRunTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayRunTargetStatus next)
    {
        switch (Status)
        {
            case LegacyPayRunTargetStatus.Created:
                switch (next)
                {
                    case LegacyPayRunTargetStatus.Retired:
                        return true;
                }
                return false;
            case LegacyPayRunTargetStatus.Validated:
                return next == LegacyPayRunTargetStatus.Retired;
            case LegacyPayRunTargetStatus.Published:
                return next == LegacyPayRunTargetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LegacyPayRunTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay run target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy pay run target"} {Code}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public LegacyPayRunTarget Copy()
    {
        return new LegacyPayRunTarget(Code, Name, Badge, Ratio, Unit, Checksum, ContactEmail, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
