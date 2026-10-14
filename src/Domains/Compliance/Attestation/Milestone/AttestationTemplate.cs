namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation template is in its life cycle.</summary>
public enum AttestationTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a attestation template and its current status.</summary>
public sealed class AttestationTemplate(string code, string title, double discount, int batchSize, double rate, AttestationTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public int BatchSize { get; set; } = batchSize;
    public double Rate { get; set; } = rate;
    public AttestationTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a attestation template in status requested.</summary>
    public static AttestationTemplate NewAttestationTemplate(string code, string title)
    {
        return new AttestationTemplate(code, title, 0.0, 0, 0.0, AttestationTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationTemplateStatus next)
    {
        switch (Status)
        {
            case AttestationTemplateStatus.Requested:
                switch (next)
                {
                    case AttestationTemplateStatus.Expired:
                        return true;
                }
                return false;
            case AttestationTemplateStatus.Granted:
                switch (next)
                {
                    case AttestationTemplateStatus.Revoked:
                        return true;
                }
                return false;
            case AttestationTemplateStatus.Revoked:
                switch (next)
                {
                    case AttestationTemplateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount + (double)BatchSize, 2);
    }

    public AttestationTemplate Copy()
    {
        return new AttestationTemplate(Code, Title, Discount, BatchSize, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
