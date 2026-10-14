namespace Acme.Workforce.Payrollops.Deduction.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction budget is in its life cycle.</summary>
public enum DeductionBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One deduction budget, with what the planners need to act on it.</summary>
public sealed class DeductionBudget(string code, string title, int limit, double margin, int tenureMonths, List<string> tags, string department, int expiresAt, DeductionBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public double Margin { get; set; } = margin;
    public int TenureMonths { get; set; } = tenureMonths;
    public List<string> Tags { get; set; } = tags;
    public string Department { get; set; } = department;
    public int ExpiresAt { get; set; } = expiresAt;
    public DeductionBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a deduction budget in status requested.</summary>
    public static DeductionBudget NewDeductionBudget(string code, string title)
    {
        return new DeductionBudget(
            code,
            title,
            0,
            0.0,
            0,
            new List<string>(),
            "",
            0,
            DeductionBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionBudgetStatus next)
    {
        switch (Status)
        {
            case DeductionBudgetStatus.Requested:
                switch (next)
                {
                    case DeductionBudgetStatus.Expired:
                        return true;
                }
                return false;
            case DeductionBudgetStatus.Granted:
                return next == DeductionBudgetStatus.Revoked || next == DeductionBudgetStatus.Expired;
            case DeductionBudgetStatus.Revoked:
                switch (next)
                {
                    case DeductionBudgetStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"deduction budget"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Limit + (double)ExpiresAt, 2);
    }

    public DeductionBudget Copy()
    {
        return new DeductionBudget(Code, Title, Limit, Margin, TenureMonths, Tags, Department, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
