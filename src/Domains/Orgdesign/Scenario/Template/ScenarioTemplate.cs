namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario template is in its life cycle.</summary>
public enum ScenarioTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scenario template. Its code is unique within a tenant.</summary>
public sealed class ScenarioTemplate(string code, string label, string jobCode, bool active, string phone, double rating, int limit, double ceiling, ScenarioTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public bool Active { get; set; } = active;
    public string Phone { get; set; } = phone;
    public double Rating { get; set; } = rating;
    public int Limit { get; set; } = limit;
    public double Ceiling { get; set; } = ceiling;
    public ScenarioTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a scenario template in status open.</summary>
    public static ScenarioTemplate NewScenarioTemplate(string code, string label)
    {
        return new ScenarioTemplate(code, label, "", false, "", 0.0, 0, 0.0, ScenarioTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioTemplateStatus next)
    {
        switch (Status)
        {
            case ScenarioTemplateStatus.Open:
                return next == ScenarioTemplateStatus.Resolved;
            case ScenarioTemplateStatus.InReview:
                switch (next)
                {
                    case ScenarioTemplateStatus.Resolved:
                        return true;
                }
                return false;
            case ScenarioTemplateStatus.Resolved:
                switch (next)
                {
                    case ScenarioTemplateStatus.Archived:
                        return true;
                }
                return false;
            case ScenarioTemplateStatus.Reopened:
                return next == ScenarioTemplateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScenarioTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Limit - Ceiling, 2);
    }

    public ScenarioTemplate Copy()
    {
        return new ScenarioTemplate(Code, Label, JobCode, Active, Phone, Rating, Limit, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
