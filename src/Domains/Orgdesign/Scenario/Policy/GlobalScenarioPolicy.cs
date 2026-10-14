namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global scenario policy is in its life cycle.</summary>
public enum GlobalScenarioPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a global scenario policy and its current status.</summary>
public sealed class GlobalScenarioPolicy(string code, string name, string grade, int priority, bool approved, double rate, double budget, double fte, string externalRef, GlobalScenarioPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public bool Approved { get; set; } = approved;
    public double Rate { get; set; } = rate;
    public double Budget { get; set; } = budget;
    public double Fte { get; set; } = fte;
    public string ExternalRef { get; set; } = externalRef;
    public GlobalScenarioPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a global scenario policy in status created.</summary>
    public static GlobalScenarioPolicy NewGlobalScenarioPolicy(string code, string name)
    {
        return new GlobalScenarioPolicy(
            code,
            name,
            "",
            0,
            false,
            0.0,
            0.0,
            0.0,
            "",
            GlobalScenarioPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalScenarioPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalScenarioPolicyStatus next)
    {
        switch (Status)
        {
            case GlobalScenarioPolicyStatus.Created:
                switch (next)
                {
                    case GlobalScenarioPolicyStatus.Retired:
                        return true;
                }
                return false;
            case GlobalScenarioPolicyStatus.Validated:
                return next == GlobalScenarioPolicyStatus.Published || next == GlobalScenarioPolicyStatus.Retired;
            case GlobalScenarioPolicyStatus.Published:
                switch (next)
                {
                    case GlobalScenarioPolicyStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalScenarioPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global scenario policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetTotal()
    {
        return Math.Round(Rate + Fte, 2);
    }

    public GlobalScenarioPolicy Copy()
    {
        return new GlobalScenarioPolicy(
            Code,
            Name,
            Grade,
            Priority,
            Approved,
            Rate,
            Budget,
            Fte,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
