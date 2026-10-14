namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit baseline is in its life cycle.</summary>
public enum AuditBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a audit baseline and its current status.</summary>
public sealed class AuditBaseline(string code, string name, double floorValue, string employeeId, string region, AuditBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string EmployeeId { get; set; } = employeeId;
    public string Region { get; set; } = region;
    public AuditBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a audit baseline in status created.</summary>
    public static AuditBaseline NewAuditBaseline(string code, string name)
    {
        return new AuditBaseline(code, name, 0.0, "", "", AuditBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditBaselineStatus next)
    {
        switch (Status)
        {
            case AuditBaselineStatus.Created:
                switch (next)
                {
                    case AuditBaselineStatus.Validated:
                        return true;
                }
                return false;
            case AuditBaselineStatus.Validated:
                switch (next)
                {
                    case AuditBaselineStatus.Published:
                    case AuditBaselineStatus.Retired:
                        return true;
                }
                return false;
            case AuditBaselineStatus.Published:
                switch (next)
                {
                    case AuditBaselineStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public AuditBaseline Copy()
    {
        return new AuditBaseline(Code, Name, FloorValue, EmployeeId, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
