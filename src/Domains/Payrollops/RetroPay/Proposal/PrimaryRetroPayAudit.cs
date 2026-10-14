namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary retro pay audit is in its life cycle.</summary>
public enum PrimaryRetroPayAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A primary retro pay audit as the retro pay team keeps it.</summary>
public sealed class PrimaryRetroPayAudit(string code, string label, string timezone, Dictionary<string, string> attributes, double ratio, PrimaryRetroPayAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ratio { get; set; } = ratio;
    public PrimaryRetroPayAuditStatus Status { get; set; } = status;

    /// <summary>Starts a primary retro pay audit in status created.</summary>
    public static PrimaryRetroPayAudit NewPrimaryRetroPayAudit(string code, string label)
    {
        return new PrimaryRetroPayAudit(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            0.0,
            PrimaryRetroPayAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRetroPayAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRetroPayAuditStatus next)
    {
        switch (Status)
        {
            case PrimaryRetroPayAuditStatus.Created:
                return next == PrimaryRetroPayAuditStatus.Published;
            case PrimaryRetroPayAuditStatus.Validated:
                return next == PrimaryRetroPayAuditStatus.Retired;
            case PrimaryRetroPayAuditStatus.Published:
                return next == PrimaryRetroPayAuditStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryRetroPayAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary retro pay audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public PrimaryRetroPayAudit Copy()
    {
        return new PrimaryRetroPayAudit(Code, Label, Timezone, Attributes, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
