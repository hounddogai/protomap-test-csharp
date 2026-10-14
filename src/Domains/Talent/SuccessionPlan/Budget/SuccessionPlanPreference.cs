namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan preference is in its life cycle.</summary>
public enum SuccessionPlanPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a succession plan preference and its current status.</summary>
public sealed class SuccessionPlanPreference(string code, string label, string team, double fte, double threshold, int batchSize, SuccessionPlanPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public double Fte { get; set; } = fte;
    public double Threshold { get; set; } = threshold;
    public int BatchSize { get; set; } = batchSize;
    public SuccessionPlanPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan preference in status idle.</summary>
    public static SuccessionPlanPreference NewSuccessionPlanPreference(string code, string label)
    {
        return new SuccessionPlanPreference(code, label, "", 0.0, 0.0, 0, SuccessionPlanPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanPreferenceStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanPreferenceStatus.Idle:
                return next == SuccessionPlanPreferenceStatus.Draining;
            case SuccessionPlanPreferenceStatus.Warming:
                return next == SuccessionPlanPreferenceStatus.Stopped;
            case SuccessionPlanPreferenceStatus.Ready:
                switch (next)
                {
                    case SuccessionPlanPreferenceStatus.Draining:
                    case SuccessionPlanPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case SuccessionPlanPreferenceStatus.Draining:
                return next == SuccessionPlanPreferenceStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"succession plan preference"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Threshold * (double)BatchSize, 2);
    }

    public SuccessionPlanPreference Copy()
    {
        return new SuccessionPlanPreference(Code, Label, Team, Fte, Threshold, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
