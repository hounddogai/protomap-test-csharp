namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification estimate is in its life cycle.</summary>
public enum CertificationEstimateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A certification estimate as the certification team keeps it.</summary>
public sealed class CertificationEstimate(string code, string title, int expiresAt, string department, string name, bool active, string label, CertificationEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Department { get; set; } = department;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string Label { get; set; } = label;
    public CertificationEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a certification estimate in status new.</summary>
    public static CertificationEstimate NewCertificationEstimate(string code, string title)
    {
        return new CertificationEstimate(code, title, 0, "", "", false, "", CertificationEstimateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationEstimateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationEstimateStatus next)
    {
        switch (Status)
        {
            case CertificationEstimateStatus.New:
                switch (next)
                {
                    case CertificationEstimateStatus.Queued:
                        return true;
                }
                return false;
            case CertificationEstimateStatus.Queued:
                switch (next)
                {
                    case CertificationEstimateStatus.Running:
                    case CertificationEstimateStatus.Done:
                        return true;
                }
                return false;
            case CertificationEstimateStatus.Running:
                return next == CertificationEstimateStatus.Done || next == CertificationEstimateStatus.Failed;
            case CertificationEstimateStatus.Done:
                return next == CertificationEstimateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CertificationEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"certification estimate"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public CertificationEstimate Copy()
    {
        return new CertificationEstimate(Code, Title, ExpiresAt, Department, Name, Active, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
