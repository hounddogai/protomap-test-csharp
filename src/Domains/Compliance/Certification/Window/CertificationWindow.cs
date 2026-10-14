namespace Acme.Workforce.Compliance.Certification.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification window is in its life cycle.</summary>
public enum CertificationWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A certification window. Its code is unique within a tenant.</summary>
public sealed class CertificationWindow(string code, string label, int retries, int windowDays, double weight, double ratio, double rate, int quantity, string category, CertificationWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int WindowDays { get; set; } = windowDays;
    public double Weight { get; set; } = weight;
    public double Ratio { get; set; } = ratio;
    public double Rate { get; set; } = rate;
    public int Quantity { get; set; } = quantity;
    public string Category { get; set; } = category;
    public CertificationWindowStatus Status { get; set; } = status;

    /// <summary>Starts a certification window in status new.</summary>
    public static CertificationWindow NewCertificationWindow(string code, string label)
    {
        return new CertificationWindow(code, label, 0, 0, 0.0, 0.0, 0.0, 0, "", CertificationWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationWindowStatus next)
    {
        switch (Status)
        {
            case CertificationWindowStatus.New:
                switch (next)
                {
                    case CertificationWindowStatus.Running:
                    case CertificationWindowStatus.Done:
                        return true;
                }
                return false;
            case CertificationWindowStatus.Queued:
                return next == CertificationWindowStatus.Running;
            case CertificationWindowStatus.Running:
                return next == CertificationWindowStatus.Done;
            case CertificationWindowStatus.Done:
                switch (next)
                {
                    case CertificationWindowStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"certification window"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Ratio - Rate, 2);
    }

    public CertificationWindow Copy()
    {
        return new CertificationWindow(
            Code,
            Label,
            Retries,
            WindowDays,
            Weight,
            Ratio,
            Rate,
            Quantity,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
