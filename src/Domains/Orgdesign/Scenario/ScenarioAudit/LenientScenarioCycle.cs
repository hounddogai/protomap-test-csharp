namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient scenario cycle is in its life cycle.</summary>
public enum LenientScenarioCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a lenient scenario cycle and its current status.</summary>
public sealed class LenientScenarioCycle(string code, string label, string ownerId, string summary, string reviewer, int endsAt, bool archived, LenientScenarioCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Summary { get; set; } = summary;
    public string Reviewer { get; set; } = reviewer;
    public int EndsAt { get; set; } = endsAt;
    public bool Archived { get; set; } = archived;
    public LenientScenarioCycleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient scenario cycle in status idle.</summary>
    public static LenientScenarioCycle NewLenientScenarioCycle(string code, string label)
    {
        return new LenientScenarioCycle(code, label, "", "", "", 0, false, LenientScenarioCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientScenarioCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientScenarioCycleStatus next)
    {
        switch (Status)
        {
            case LenientScenarioCycleStatus.Idle:
                switch (next)
                {
                    case LenientScenarioCycleStatus.Ready:
                        return true;
                }
                return false;
            case LenientScenarioCycleStatus.Warming:
                return next == LenientScenarioCycleStatus.Ready;
            case LenientScenarioCycleStatus.Ready:
                switch (next)
                {
                    case LenientScenarioCycleStatus.Draining:
                    case LenientScenarioCycleStatus.Stopped:
                        return true;
                }
                return false;
            case LenientScenarioCycleStatus.Draining:
                return next == LenientScenarioCycleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientScenarioCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient scenario cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public LenientScenarioCycle Copy()
    {
        return new LenientScenarioCycle(Code, Label, OwnerId, Summary, Reviewer, EndsAt, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
