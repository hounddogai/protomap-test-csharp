namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional audit draft is in its life cycle.</summary>
public enum RegionalAuditDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a regional audit draft and its current status.</summary>
public sealed class RegionalAuditDraft(string code, string label, int createdAt, string locale, string channel, string contactEmail, double fte, RegionalAuditDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public string Locale { get; set; } = locale;
    public string Channel { get; set; } = channel;
    public string ContactEmail { get; set; } = contactEmail;
    public double Fte { get; set; } = fte;
    public RegionalAuditDraftStatus Status { get; set; } = status;

    /// <summary>Starts a regional audit draft in status idle.</summary>
    public static RegionalAuditDraft NewRegionalAuditDraft(string code, string label)
    {
        return new RegionalAuditDraft(code, label, 0, "", "", "", 0.0, RegionalAuditDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAuditDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAuditDraftStatus next)
    {
        switch (Status)
        {
            case RegionalAuditDraftStatus.Idle:
                return next == RegionalAuditDraftStatus.Warming || next == RegionalAuditDraftStatus.Stopped;
            case RegionalAuditDraftStatus.Warming:
                switch (next)
                {
                    case RegionalAuditDraftStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalAuditDraftStatus.Ready:
                return next == RegionalAuditDraftStatus.Draining || next == RegionalAuditDraftStatus.Stopped;
            case RegionalAuditDraftStatus.Draining:
                return next == RegionalAuditDraftStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalAuditDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional audit draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Fte + (double)CreatedAt, 2);
    }

    public RegionalAuditDraft Copy()
    {
        return new RegionalAuditDraft(Code, Label, CreatedAt, Locale, Channel, ContactEmail, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
