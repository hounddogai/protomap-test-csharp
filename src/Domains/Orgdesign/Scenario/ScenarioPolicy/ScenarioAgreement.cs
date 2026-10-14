namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario agreement is in its life cycle.</summary>
public enum ScenarioAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a scenario agreement and its current status.</summary>
public sealed class ScenarioAgreement(string code, string name, int batchSize, int priority, int quantity, bool pinned, string label, ScenarioAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public int Priority { get; set; } = priority;
    public int Quantity { get; set; } = quantity;
    public bool Pinned { get; set; } = pinned;
    public string Label { get; set; } = label;
    public ScenarioAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a scenario agreement in status new.</summary>
    public static ScenarioAgreement NewScenarioAgreement(string code, string name)
    {
        return new ScenarioAgreement(code, name, 0, 0, 0, false, "", ScenarioAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioAgreementStatus next)
    {
        switch (Status)
        {
            case ScenarioAgreementStatus.New:
                return next == ScenarioAgreementStatus.Queued || next == ScenarioAgreementStatus.Done;
            case ScenarioAgreementStatus.Queued:
                switch (next)
                {
                    case ScenarioAgreementStatus.Done:
                        return true;
                }
                return false;
            case ScenarioAgreementStatus.Running:
                switch (next)
                {
                    case ScenarioAgreementStatus.Done:
                    case ScenarioAgreementStatus.Failed:
                        return true;
                }
                return false;
            case ScenarioAgreementStatus.Done:
                switch (next)
                {
                    case ScenarioAgreementStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scenario agreement"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Priority - (double)Quantity, 2);
    }

    public ScenarioAgreement Copy()
    {
        return new ScenarioAgreement(Code, Name, BatchSize, Priority, Quantity, Pinned, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
