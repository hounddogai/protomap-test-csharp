namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey allocation is in its life cycle.</summary>
public enum PulseSurveyAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pulse survey allocation as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyAllocation(string code, string label, List<string> tags, double ratio, string externalRef, bool approved, int durationMinutes, PulseSurveyAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public double Ratio { get; set; } = ratio;
    public string ExternalRef { get; set; } = externalRef;
    public bool Approved { get; set; } = approved;
    public int DurationMinutes { get; set; } = durationMinutes;
    public PulseSurveyAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey allocation in status idle.</summary>
    public static PulseSurveyAllocation NewPulseSurveyAllocation(string code, string label)
    {
        return new PulseSurveyAllocation(
            code,
            label,
            new List<string>(),
            0.0,
            "",
            false,
            0,
            PulseSurveyAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyAllocationStatus next)
    {
        switch (Status)
        {
            case PulseSurveyAllocationStatus.Idle:
                return next == PulseSurveyAllocationStatus.Stopped;
            case PulseSurveyAllocationStatus.Warming:
                return next == PulseSurveyAllocationStatus.Draining || next == PulseSurveyAllocationStatus.Stopped;
            case PulseSurveyAllocationStatus.Ready:
                return next == PulseSurveyAllocationStatus.Draining || next == PulseSurveyAllocationStatus.Stopped;
            case PulseSurveyAllocationStatus.Draining:
                return next == PulseSurveyAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PulseSurveyAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetValue()
    {
        return Math.Round(Ratio + (double)DurationMinutes, 2);
    }

    public PulseSurveyAllocation Copy()
    {
        return new PulseSurveyAllocation(Code, Label, Tags, Ratio, ExternalRef, Approved, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
