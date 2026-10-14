namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey template is in its life cycle.</summary>
public enum PulseSurveyTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a pulse survey template and its current status.</summary>
public sealed class PulseSurveyTemplate(string code, string label, double budget, string category, string segment, PulseSurveyTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Category { get; set; } = category;
    public string Segment { get; set; } = segment;
    public PulseSurveyTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey template in status new.</summary>
    public static PulseSurveyTemplate NewPulseSurveyTemplate(string code, string label)
    {
        return new PulseSurveyTemplate(code, label, 0.0, "", "", PulseSurveyTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyTemplateStatus next)
    {
        switch (Status)
        {
            case PulseSurveyTemplateStatus.New:
                switch (next)
                {
                    case PulseSurveyTemplateStatus.Failed:
                        return true;
                }
                return false;
            case PulseSurveyTemplateStatus.Queued:
                return next == PulseSurveyTemplateStatus.Running;
            case PulseSurveyTemplateStatus.Running:
                return next == PulseSurveyTemplateStatus.Done || next == PulseSurveyTemplateStatus.Failed;
            case PulseSurveyTemplateStatus.Done:
                return next == PulseSurveyTemplateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PulseSurveyTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pulse survey template"} {Code}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public PulseSurveyTemplate Copy()
    {
        return new PulseSurveyTemplate(Code, Label, Budget, Category, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
