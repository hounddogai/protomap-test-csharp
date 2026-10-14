namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default audit mapping is in its life cycle.</summary>
public enum DefaultAuditMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a default audit mapping and its current status.</summary>
public sealed class DefaultAuditMapping(string code, string name, int tenureMonths, string source, double hours, bool approved, string salaryBand, DefaultAuditMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Source { get; set; } = source;
    public double Hours { get; set; } = hours;
    public bool Approved { get; set; } = approved;
    public string SalaryBand { get; set; } = salaryBand;
    public DefaultAuditMappingStatus Status { get; set; } = status;

    /// <summary>Starts a default audit mapping in status draft.</summary>
    public static DefaultAuditMapping NewDefaultAuditMapping(string code, string name)
    {
        return new DefaultAuditMapping(code, name, 0, "", 0.0, false, "", DefaultAuditMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAuditMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAuditMappingStatus next)
    {
        switch (Status)
        {
            case DefaultAuditMappingStatus.Draft:
                switch (next)
                {
                    case DefaultAuditMappingStatus.Pending:
                    case DefaultAuditMappingStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultAuditMappingStatus.Pending:
                switch (next)
                {
                    case DefaultAuditMappingStatus.Suspended:
                    case DefaultAuditMappingStatus.Closed:
                        return true;
                }
                return false;
            case DefaultAuditMappingStatus.Active:
                switch (next)
                {
                    case DefaultAuditMappingStatus.Suspended:
                    case DefaultAuditMappingStatus.Closed:
                        return true;
                }
                return false;
            case DefaultAuditMappingStatus.Suspended:
                return next == DefaultAuditMappingStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultAuditMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default audit mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Hours * (double)TenureMonths, 2);
    }

    public DefaultAuditMapping Copy()
    {
        return new DefaultAuditMapping(Code, Name, TenureMonths, Source, Hours, Approved, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
