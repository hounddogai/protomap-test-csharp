namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped scenario estimate is in its life cycle.</summary>
public enum CappedScenarioEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A capped scenario estimate as the scenario team keeps it.</summary>
public sealed class CappedScenarioEstimate(string code, string title, string employeeId, string notes, string salaryBand, double weight, double ratio, int limit, CappedScenarioEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Notes { get; set; } = notes;
    public string SalaryBand { get; set; } = salaryBand;
    public double Weight { get; set; } = weight;
    public double Ratio { get; set; } = ratio;
    public int Limit { get; set; } = limit;
    public CappedScenarioEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a capped scenario estimate in status planned.</summary>
    public static CappedScenarioEstimate NewCappedScenarioEstimate(string code, string title)
    {
        return new CappedScenarioEstimate(
            code,
            title,
            "",
            "",
            "",
            0.0,
            0.0,
            0,
            CappedScenarioEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedScenarioEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedScenarioEstimateStatus next)
    {
        switch (Status)
        {
            case CappedScenarioEstimateStatus.Planned:
                return next == CappedScenarioEstimateStatus.Scheduled;
            case CappedScenarioEstimateStatus.Scheduled:
                return next == CappedScenarioEstimateStatus.InProgress || next == CappedScenarioEstimateStatus.Cancelled;
            case CappedScenarioEstimateStatus.InProgress:
                switch (next)
                {
                    case CappedScenarioEstimateStatus.Completed:
                        return true;
                }
                return false;
            case CappedScenarioEstimateStatus.Completed:
                return next == CappedScenarioEstimateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CappedScenarioEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped scenario estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped scenario estimate"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Limit + Ratio, 2);
    }

    public CappedScenarioEstimate Copy()
    {
        return new CappedScenarioEstimate(Code, Title, EmployeeId, Notes, SalaryBand, Weight, Ratio, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
