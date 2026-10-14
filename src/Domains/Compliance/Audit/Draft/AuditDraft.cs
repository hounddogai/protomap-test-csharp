namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit draft is in its life cycle.</summary>
public enum AuditDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a audit draft and its current status.</summary>
public sealed class AuditDraft(string code, string label, double weight, string timezone, string phone, string country, double amount, string managerId, string title, string summary, AuditDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public string Timezone { get; set; } = timezone;
    public string Phone { get; set; } = phone;
    public string Country { get; set; } = country;
    public double Amount { get; set; } = amount;
    public string ManagerId { get; set; } = managerId;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public AuditDraftStatus Status { get; set; } = status;

    /// <summary>Starts a audit draft in status draft.</summary>
    public static AuditDraft NewAuditDraft(string code, string label)
    {
        return new AuditDraft(code, label, 0.0, "", "", "", 0.0, "", "", "", AuditDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditDraftStatus next)
    {
        switch (Status)
        {
            case AuditDraftStatus.Draft:
                return next == AuditDraftStatus.Active;
            case AuditDraftStatus.Pending:
                return next == AuditDraftStatus.Suspended;
            case AuditDraftStatus.Active:
                switch (next)
                {
                    case AuditDraftStatus.Suspended:
                        return true;
                }
                return false;
            case AuditDraftStatus.Suspended:
                return next == AuditDraftStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AuditDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Weight - Amount, 2);
    }

    public AuditDraft Copy()
    {
        return new AuditDraft(
            Code,
            Label,
            Weight,
            Timezone,
            Phone,
            Country,
            Amount,
            ManagerId,
            Title,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
