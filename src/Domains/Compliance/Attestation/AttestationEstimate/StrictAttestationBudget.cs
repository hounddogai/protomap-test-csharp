namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict attestation budget is in its life cycle.</summary>
public enum StrictAttestationBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One strict attestation budget, with what the planners need to act on it.</summary>
public sealed class StrictAttestationBudget(string code, string label, int durationMinutes, double cost, string summary, int limit, string notes, string skill, string employeeId, double floorValue, string contactEmail, StrictAttestationBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Cost { get; set; } = cost;
    public string Summary { get; set; } = summary;
    public int Limit { get; set; } = limit;
    public string Notes { get; set; } = notes;
    public string Skill { get; set; } = skill;
    public string EmployeeId { get; set; } = employeeId;
    public double FloorValue { get; set; } = floorValue;
    public string ContactEmail { get; set; } = contactEmail;
    public StrictAttestationBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a strict attestation budget in status idle.</summary>
    public static StrictAttestationBudget NewStrictAttestationBudget(string code, string label)
    {
        return new StrictAttestationBudget(
            code,
            label,
            0,
            0.0,
            "",
            0,
            "",
            "",
            "",
            0.0,
            "",
            StrictAttestationBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAttestationBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAttestationBudgetStatus next)
    {
        switch (Status)
        {
            case StrictAttestationBudgetStatus.Idle:
                return next == StrictAttestationBudgetStatus.Warming;
            case StrictAttestationBudgetStatus.Warming:
                return next == StrictAttestationBudgetStatus.Stopped;
            case StrictAttestationBudgetStatus.Ready:
                switch (next)
                {
                    case StrictAttestationBudgetStatus.Draining:
                        return true;
                }
                return false;
            case StrictAttestationBudgetStatus.Draining:
                return next == StrictAttestationBudgetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictAttestationBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict attestation budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round(FloorValue - Cost, 2);
    }

    public StrictAttestationBudget Copy()
    {
        return new StrictAttestationBudget(
            Code,
            Label,
            DurationMinutes,
            Cost,
            Summary,
            Limit,
            Notes,
            Skill,
            EmployeeId,
            FloorValue,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
