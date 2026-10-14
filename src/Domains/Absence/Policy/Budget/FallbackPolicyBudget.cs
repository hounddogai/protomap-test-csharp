namespace Acme.Workforce.Absence.Policy.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback policy budget is in its life cycle.</summary>
public enum FallbackPolicyBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback policy budget. Its code is unique within a tenant.</summary>
public sealed class FallbackPolicyBudget(string code, string name, string employeeId, double cost, double fte, string contactEmail, int endsAt, double minimum, string department, string externalRef, string category, FallbackPolicyBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public double Cost { get; set; } = cost;
    public double Fte { get; set; } = fte;
    public string ContactEmail { get; set; } = contactEmail;
    public int EndsAt { get; set; } = endsAt;
    public double Minimum { get; set; } = minimum;
    public string Department { get; set; } = department;
    public string ExternalRef { get; set; } = externalRef;
    public string Category { get; set; } = category;
    public FallbackPolicyBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a fallback policy budget in status created.</summary>
    public static FallbackPolicyBudget NewFallbackPolicyBudget(string code, string name)
    {
        return new FallbackPolicyBudget(
            code,
            name,
            "",
            0.0,
            0.0,
            "",
            0,
            0.0,
            "",
            "",
            "",
            FallbackPolicyBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPolicyBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPolicyBudgetStatus next)
    {
        switch (Status)
        {
            case FallbackPolicyBudgetStatus.Created:
                switch (next)
                {
                    case FallbackPolicyBudgetStatus.Validated:
                    case FallbackPolicyBudgetStatus.Retired:
                        return true;
                }
                return false;
            case FallbackPolicyBudgetStatus.Validated:
                switch (next)
                {
                    case FallbackPolicyBudgetStatus.Published:
                    case FallbackPolicyBudgetStatus.Retired:
                        return true;
                }
                return false;
            case FallbackPolicyBudgetStatus.Published:
                return next == FallbackPolicyBudgetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FallbackPolicyBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback policy budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback policy budget"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Minimum + (double)EndsAt, 2);
    }

    public FallbackPolicyBudget Copy()
    {
        return new FallbackPolicyBudget(
            Code,
            Name,
            EmployeeId,
            Cost,
            Fte,
            ContactEmail,
            EndsAt,
            Minimum,
            Department,
            ExternalRef,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
