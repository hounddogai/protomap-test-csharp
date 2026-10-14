namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy tax form assignment is in its life cycle.</summary>
public enum LegacyTaxFormAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy tax form assignment as the tax form team keeps it.</summary>
public sealed class LegacyTaxFormAssignment(string code, string label, string description, string contactEmail, string category, string team, int expiresAt, LegacyTaxFormAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public string ContactEmail { get; set; } = contactEmail;
    public string Category { get; set; } = category;
    public string Team { get; set; } = team;
    public int ExpiresAt { get; set; } = expiresAt;
    public LegacyTaxFormAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy tax form assignment in status open.</summary>
    public static LegacyTaxFormAssignment NewLegacyTaxFormAssignment(string code, string label)
    {
        return new LegacyTaxFormAssignment(code, label, "", "", "", "", 0, LegacyTaxFormAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTaxFormAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTaxFormAssignmentStatus next)
    {
        switch (Status)
        {
            case LegacyTaxFormAssignmentStatus.Open:
                switch (next)
                {
                    case LegacyTaxFormAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case LegacyTaxFormAssignmentStatus.InReview:
                switch (next)
                {
                    case LegacyTaxFormAssignmentStatus.Resolved:
                    case LegacyTaxFormAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case LegacyTaxFormAssignmentStatus.Resolved:
                return next == LegacyTaxFormAssignmentStatus.Reopened || next == LegacyTaxFormAssignmentStatus.Archived;
            case LegacyTaxFormAssignmentStatus.Reopened:
                switch (next)
                {
                    case LegacyTaxFormAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTaxFormAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy tax form assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public LegacyTaxFormAssignment Copy()
    {
        return new LegacyTaxFormAssignment(
            Code,
            Label,
            Description,
            ContactEmail,
            Category,
            Team,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
