namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual audit mapping is in its life cycle.</summary>
public enum ManualAuditMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a manual audit mapping and its current status.</summary>
public sealed class ManualAuditMapping(string code, string label, string title, bool active, double rating, string phone, string grade, string description, string name, int batchSize, string category, ManualAuditMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public string Phone { get; set; } = phone;
    public string Grade { get; set; } = grade;
    public string Description { get; set; } = description;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public string Category { get; set; } = category;
    public ManualAuditMappingStatus Status { get; set; } = status;

    /// <summary>Starts a manual audit mapping in status draft.</summary>
    public static ManualAuditMapping NewManualAuditMapping(string code, string label)
    {
        return new ManualAuditMapping(
            code,
            label,
            "",
            false,
            0.0,
            "",
            "",
            "",
            "",
            0,
            "",
            ManualAuditMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAuditMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAuditMappingStatus next)
    {
        switch (Status)
        {
            case ManualAuditMappingStatus.Draft:
                switch (next)
                {
                    case ManualAuditMappingStatus.Pending:
                    case ManualAuditMappingStatus.Closed:
                        return true;
                }
                return false;
            case ManualAuditMappingStatus.Pending:
                return next == ManualAuditMappingStatus.Active || next == ManualAuditMappingStatus.Suspended;
            case ManualAuditMappingStatus.Active:
                switch (next)
                {
                    case ManualAuditMappingStatus.Suspended:
                    case ManualAuditMappingStatus.Closed:
                        return true;
                }
                return false;
            case ManualAuditMappingStatus.Suspended:
                return next == ManualAuditMappingStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualAuditMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual audit mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Rating - (double)BatchSize, 2);
    }

    public ManualAuditMapping Copy()
    {
        return new ManualAuditMapping(
            Code,
            Label,
            Title,
            Active,
            Rating,
            Phone,
            Grade,
            Description,
            Name,
            BatchSize,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
