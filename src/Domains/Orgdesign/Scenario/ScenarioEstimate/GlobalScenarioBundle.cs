namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global scenario bundle is in its life cycle.</summary>
public enum GlobalScenarioBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A global scenario bundle. Its code is unique within a tenant.</summary>
public sealed class GlobalScenarioBundle(string code, string label, string country, int revision, string employeeId, GlobalScenarioBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public int Revision { get; set; } = revision;
    public string EmployeeId { get; set; } = employeeId;
    public GlobalScenarioBundleStatus Status { get; set; } = status;

    /// <summary>Starts a global scenario bundle in status draft.</summary>
    public static GlobalScenarioBundle NewGlobalScenarioBundle(string code, string label)
    {
        return new GlobalScenarioBundle(code, label, "", 0, "", GlobalScenarioBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalScenarioBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalScenarioBundleStatus next)
    {
        switch (Status)
        {
            case GlobalScenarioBundleStatus.Draft:
                switch (next)
                {
                    case GlobalScenarioBundleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalScenarioBundleStatus.Pending:
                switch (next)
                {
                    case GlobalScenarioBundleStatus.Suspended:
                    case GlobalScenarioBundleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalScenarioBundleStatus.Active:
                switch (next)
                {
                    case GlobalScenarioBundleStatus.Suspended:
                    case GlobalScenarioBundleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalScenarioBundleStatus.Suspended:
                return next == GlobalScenarioBundleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GlobalScenarioBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global scenario bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public GlobalScenarioBundle Copy()
    {
        return new GlobalScenarioBundle(Code, Label, Country, Revision, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
