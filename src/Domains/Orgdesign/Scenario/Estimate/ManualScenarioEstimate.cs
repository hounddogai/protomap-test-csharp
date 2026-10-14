namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual scenario estimate is in its life cycle.</summary>
public enum ManualScenarioEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One manual scenario estimate, with what the planners need to act on it.</summary>
public sealed class ManualScenarioEstimate(string code, string name, string salaryBand, double weight, string location, bool archived, ManualScenarioEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public double Weight { get; set; } = weight;
    public string Location { get; set; } = location;
    public bool Archived { get; set; } = archived;
    public ManualScenarioEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a manual scenario estimate in status requested.</summary>
    public static ManualScenarioEstimate NewManualScenarioEstimate(string code, string name)
    {
        return new ManualScenarioEstimate(code, name, "", 0.0, "", false, ManualScenarioEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualScenarioEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualScenarioEstimateStatus next)
    {
        switch (Status)
        {
            case ManualScenarioEstimateStatus.Requested:
                return next == ManualScenarioEstimateStatus.Revoked;
            case ManualScenarioEstimateStatus.Granted:
                switch (next)
                {
                    case ManualScenarioEstimateStatus.Revoked:
                    case ManualScenarioEstimateStatus.Expired:
                        return true;
                }
                return false;
            case ManualScenarioEstimateStatus.Revoked:
                return next == ManualScenarioEstimateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ManualScenarioEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual scenario estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public ManualScenarioEstimate Copy()
    {
        return new ManualScenarioEstimate(Code, Name, SalaryBand, Weight, Location, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
