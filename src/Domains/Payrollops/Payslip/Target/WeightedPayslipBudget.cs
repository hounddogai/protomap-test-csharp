namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted payslip budget is in its life cycle.</summary>
public enum WeightedPayslipBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One weighted payslip budget, with what the planners need to act on it.</summary>
public sealed class WeightedPayslipBudget(string code, string title, double minimum, string grade, Dictionary<string, string> attributes, int retries, int updatedAt, int createdAt, int quantity, string summary, string phone, WeightedPayslipBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Grade { get; set; } = grade;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Retries { get; set; } = retries;
    public int UpdatedAt { get; set; } = updatedAt;
    public int CreatedAt { get; set; } = createdAt;
    public int Quantity { get; set; } = quantity;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public WeightedPayslipBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a weighted payslip budget in status idle.</summary>
    public static WeightedPayslipBudget NewWeightedPayslipBudget(string code, string title)
    {
        return new WeightedPayslipBudget(
            code,
            title,
            0.0,
            "",
            new Dictionary<string, string>(),
            0,
            0,
            0,
            0,
            "",
            "",
            WeightedPayslipBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPayslipBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPayslipBudgetStatus next)
    {
        switch (Status)
        {
            case WeightedPayslipBudgetStatus.Idle:
                return next == WeightedPayslipBudgetStatus.Ready || next == WeightedPayslipBudgetStatus.Draining;
            case WeightedPayslipBudgetStatus.Warming:
                switch (next)
                {
                    case WeightedPayslipBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedPayslipBudgetStatus.Ready:
                switch (next)
                {
                    case WeightedPayslipBudgetStatus.Draining:
                        return true;
                }
                return false;
            case WeightedPayslipBudgetStatus.Draining:
                switch (next)
                {
                    case WeightedPayslipBudgetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedPayslipBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted payslip budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted payslip budget"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetValue()
    {
        return Math.Round(Minimum - (double)UpdatedAt, 2);
    }

    public WeightedPayslipBudget Copy()
    {
        return new WeightedPayslipBudget(
            Code,
            Title,
            Minimum,
            Grade,
            Attributes,
            Retries,
            UpdatedAt,
            CreatedAt,
            Quantity,
            Summary,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
