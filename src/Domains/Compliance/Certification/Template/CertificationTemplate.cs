namespace Acme.Workforce.Compliance.Certification.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification template is in its life cycle.</summary>
public enum CertificationTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One certification template, with what the planners need to act on it.</summary>
public sealed class CertificationTemplate(string code, string label, double ratio, string managerId, double hours, double weight, CertificationTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public string ManagerId { get; set; } = managerId;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public CertificationTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a certification template in status requested.</summary>
    public static CertificationTemplate NewCertificationTemplate(string code, string label)
    {
        return new CertificationTemplate(code, label, 0.0, "", 0.0, 0.0, CertificationTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationTemplateStatus next)
    {
        switch (Status)
        {
            case CertificationTemplateStatus.Requested:
                switch (next)
                {
                    case CertificationTemplateStatus.Granted:
                    case CertificationTemplateStatus.Revoked:
                        return true;
                }
                return false;
            case CertificationTemplateStatus.Granted:
                return next == CertificationTemplateStatus.Revoked || next == CertificationTemplateStatus.Expired;
            case CertificationTemplateStatus.Revoked:
                return next == CertificationTemplateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CertificationTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetLoad()
    {
        return Math.Round(Hours * Ratio, 2);
    }

    public CertificationTemplate Copy()
    {
        return new CertificationTemplate(Code, Label, Ratio, ManagerId, Hours, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
