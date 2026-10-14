namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey budget is in its life cycle.</summary>
public enum PulseSurveyBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pulse survey budget as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyBudget(string code, string label, string notes, string checksum, double ratio, string name, PulseSurveyBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Checksum { get; set; } = checksum;
    public double Ratio { get; set; } = ratio;
    public string Name { get; set; } = name;
    public PulseSurveyBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey budget in status draft.</summary>
    public static PulseSurveyBudget NewPulseSurveyBudget(string code, string label)
    {
        return new PulseSurveyBudget(code, label, "", "", 0.0, "", PulseSurveyBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyBudgetStatus next)
    {
        switch (Status)
        {
            case PulseSurveyBudgetStatus.Draft:
                switch (next)
                {
                    case PulseSurveyBudgetStatus.Pending:
                        return true;
                }
                return false;
            case PulseSurveyBudgetStatus.Pending:
                switch (next)
                {
                    case PulseSurveyBudgetStatus.Active:
                        return true;
                }
                return false;
            case PulseSurveyBudgetStatus.Active:
                return next == PulseSurveyBudgetStatus.Suspended || next == PulseSurveyBudgetStatus.Closed;
            case PulseSurveyBudgetStatus.Suspended:
                return next == PulseSurveyBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PulseSurveyBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pulse survey budget"} {Code}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public PulseSurveyBudget Copy()
    {
        return new PulseSurveyBudget(Code, Label, Notes, Checksum, Ratio, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
