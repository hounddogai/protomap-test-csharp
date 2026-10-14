namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy attestation template is in its life cycle.</summary>
public enum LegacyAttestationTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy attestation template. Its code is unique within a tenant.</summary>
public sealed class LegacyAttestationTemplate(string code, string name, double ratio, string channel, bool archived, string label, LegacyAttestationTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string Channel { get; set; } = channel;
    public bool Archived { get; set; } = archived;
    public string Label { get; set; } = label;
    public LegacyAttestationTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a legacy attestation template in status open.</summary>
    public static LegacyAttestationTemplate NewLegacyAttestationTemplate(string code, string name)
    {
        return new LegacyAttestationTemplate(code, name, 0.0, "", false, "", LegacyAttestationTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAttestationTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAttestationTemplateStatus next)
    {
        switch (Status)
        {
            case LegacyAttestationTemplateStatus.Open:
                return next == LegacyAttestationTemplateStatus.Reopened || next == LegacyAttestationTemplateStatus.Archived;
            case LegacyAttestationTemplateStatus.InReview:
                switch (next)
                {
                    case LegacyAttestationTemplateStatus.Resolved:
                    case LegacyAttestationTemplateStatus.Archived:
                        return true;
                }
                return false;
            case LegacyAttestationTemplateStatus.Resolved:
                return next == LegacyAttestationTemplateStatus.Reopened || next == LegacyAttestationTemplateStatus.Archived;
            case LegacyAttestationTemplateStatus.Reopened:
                switch (next)
                {
                    case LegacyAttestationTemplateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAttestationTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy attestation template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy attestation template"} {Code}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public LegacyAttestationTemplate Copy()
    {
        return new LegacyAttestationTemplate(Code, Name, Ratio, Channel, Archived, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
