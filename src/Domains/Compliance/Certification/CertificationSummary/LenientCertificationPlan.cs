namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient certification plan is in its life cycle.</summary>
public enum LenientCertificationPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A lenient certification plan. Its code is unique within a tenant.</summary>
public sealed class LenientCertificationPlan(string code, string label, int retries, string managerId, double minimum, LenientCertificationPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public string ManagerId { get; set; } = managerId;
    public double Minimum { get; set; } = minimum;
    public LenientCertificationPlanStatus Status { get; set; } = status;

    /// <summary>Starts a lenient certification plan in status new.</summary>
    public static LenientCertificationPlan NewLenientCertificationPlan(string code, string label)
    {
        return new LenientCertificationPlan(code, label, 0, "", 0.0, LenientCertificationPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientCertificationPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientCertificationPlanStatus next)
    {
        switch (Status)
        {
            case LenientCertificationPlanStatus.New:
                switch (next)
                {
                    case LenientCertificationPlanStatus.Done:
                        return true;
                }
                return false;
            case LenientCertificationPlanStatus.Queued:
                return next == LenientCertificationPlanStatus.Done || next == LenientCertificationPlanStatus.Failed;
            case LenientCertificationPlanStatus.Running:
                return next == LenientCertificationPlanStatus.Done;
            case LenientCertificationPlanStatus.Done:
                switch (next)
                {
                    case LenientCertificationPlanStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientCertificationPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient certification plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Minimum - (double)Retries, 2);
    }

    public LenientCertificationPlan Copy()
    {
        return new LenientCertificationPlan(Code, Label, Retries, ManagerId, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
