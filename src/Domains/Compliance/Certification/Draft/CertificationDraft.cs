namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification draft is in its life cycle.</summary>
public enum CertificationDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One certification draft, with what the planners need to act on it.</summary>
public sealed class CertificationDraft(string code, string label, string grade, string title, int limit, double amount, CertificationDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public double Amount { get; set; } = amount;
    public CertificationDraftStatus Status { get; set; } = status;

    /// <summary>Starts a certification draft in status idle.</summary>
    public static CertificationDraft NewCertificationDraft(string code, string label)
    {
        return new CertificationDraft(code, label, "", "", 0, 0.0, CertificationDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationDraftStatus next)
    {
        switch (Status)
        {
            case CertificationDraftStatus.Idle:
                switch (next)
                {
                    case CertificationDraftStatus.Ready:
                    case CertificationDraftStatus.Stopped:
                        return true;
                }
                return false;
            case CertificationDraftStatus.Warming:
                return next == CertificationDraftStatus.Draining;
            case CertificationDraftStatus.Ready:
                switch (next)
                {
                    case CertificationDraftStatus.Draining:
                    case CertificationDraftStatus.Stopped:
                        return true;
                }
                return false;
            case CertificationDraftStatus.Draining:
                return next == CertificationDraftStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CertificationDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Amount * (double)Limit, 2);
    }

    public CertificationDraft Copy()
    {
        return new CertificationDraft(Code, Label, Grade, Title, Limit, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
