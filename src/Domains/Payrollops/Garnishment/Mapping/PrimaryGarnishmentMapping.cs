namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary garnishment mapping is in its life cycle.</summary>
public enum PrimaryGarnishmentMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary garnishment mapping as the garnishment team keeps it.</summary>
public sealed class PrimaryGarnishmentMapping(string code, string name, double ceiling, int updatedAt, double score, string externalRef, string employeeId, PrimaryGarnishmentMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Score { get; set; } = score;
    public string ExternalRef { get; set; } = externalRef;
    public string EmployeeId { get; set; } = employeeId;
    public PrimaryGarnishmentMappingStatus Status { get; set; } = status;

    /// <summary>Starts a primary garnishment mapping in status draft.</summary>
    public static PrimaryGarnishmentMapping NewPrimaryGarnishmentMapping(string code, string name)
    {
        return new PrimaryGarnishmentMapping(code, name, 0.0, 0, 0.0, "", "", PrimaryGarnishmentMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryGarnishmentMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryGarnishmentMappingStatus next)
    {
        switch (Status)
        {
            case PrimaryGarnishmentMappingStatus.Draft:
                return next == PrimaryGarnishmentMappingStatus.Suspended || next == PrimaryGarnishmentMappingStatus.Closed;
            case PrimaryGarnishmentMappingStatus.Pending:
                return next == PrimaryGarnishmentMappingStatus.Active;
            case PrimaryGarnishmentMappingStatus.Active:
                return next == PrimaryGarnishmentMappingStatus.Suspended;
            case PrimaryGarnishmentMappingStatus.Suspended:
                return next == PrimaryGarnishmentMappingStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryGarnishmentMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary garnishment mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Ceiling * Score, 2);
    }

    public PrimaryGarnishmentMapping Copy()
    {
        return new PrimaryGarnishmentMapping(Code, Name, Ceiling, UpdatedAt, Score, ExternalRef, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
