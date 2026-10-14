namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey override is in its life cycle.</summary>
public enum PulseSurveyOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pulse survey override as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyOverride(string code, string name, string unit, string label, double ratio, string category, PulseSurveyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public string Category { get; set; } = category;
    public PulseSurveyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey override in status new.</summary>
    public static PulseSurveyOverride NewPulseSurveyOverride(string code, string name)
    {
        return new PulseSurveyOverride(code, name, "", "", 0.0, "", PulseSurveyOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyOverrideStatus next)
    {
        switch (Status)
        {
            case PulseSurveyOverrideStatus.New:
                return next == PulseSurveyOverrideStatus.Done || next == PulseSurveyOverrideStatus.Failed;
            case PulseSurveyOverrideStatus.Queued:
                switch (next)
                {
                    case PulseSurveyOverrideStatus.Running:
                    case PulseSurveyOverrideStatus.Failed:
                        return true;
                }
                return false;
            case PulseSurveyOverrideStatus.Running:
                switch (next)
                {
                    case PulseSurveyOverrideStatus.Failed:
                        return true;
                }
                return false;
            case PulseSurveyOverrideStatus.Done:
                return next == PulseSurveyOverrideStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PulseSurveyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public PulseSurveyOverride Copy()
    {
        return new PulseSurveyOverride(Code, Name, Unit, Label, Ratio, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
