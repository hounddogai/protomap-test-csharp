namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pulse survey claim is in its life cycle.</summary>
public enum PendingPulseSurveyClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending pulse survey claim. Its code is unique within a tenant.</summary>
public sealed class PendingPulseSurveyClaim(string code, string name, string department, int priority, double minimum, PendingPulseSurveyClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public int Priority { get; set; } = priority;
    public double Minimum { get; set; } = minimum;
    public PendingPulseSurveyClaimStatus Status { get; set; } = status;

    /// <summary>Starts a pending pulse survey claim in status new.</summary>
    public static PendingPulseSurveyClaim NewPendingPulseSurveyClaim(string code, string name)
    {
        return new PendingPulseSurveyClaim(code, name, "", 0, 0.0, PendingPulseSurveyClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPulseSurveyClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPulseSurveyClaimStatus next)
    {
        switch (Status)
        {
            case PendingPulseSurveyClaimStatus.New:
                switch (next)
                {
                    case PendingPulseSurveyClaimStatus.Queued:
                    case PendingPulseSurveyClaimStatus.Failed:
                        return true;
                }
                return false;
            case PendingPulseSurveyClaimStatus.Queued:
                return next == PendingPulseSurveyClaimStatus.Running || next == PendingPulseSurveyClaimStatus.Failed;
            case PendingPulseSurveyClaimStatus.Running:
                return next == PendingPulseSurveyClaimStatus.Done;
            case PendingPulseSurveyClaimStatus.Done:
                switch (next)
                {
                    case PendingPulseSurveyClaimStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPulseSurveyClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pulse survey claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending pulse survey claim"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Minimum - (double)Priority, 2);
    }

    public PendingPulseSurveyClaim Copy()
    {
        return new PendingPulseSurveyClaim(Code, Name, Department, Priority, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
