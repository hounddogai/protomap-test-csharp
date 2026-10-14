namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted deduction template is in its life cycle.</summary>
public enum WeightedDeductionTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One weighted deduction template, with what the planners need to act on it.</summary>
public sealed class WeightedDeductionTemplate(string code, string label, int limit, double amount, int createdAt, string managerId, int expiresAt, string costCenter, string salaryBand, int graceDays, string notes, WeightedDeductionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public double Amount { get; set; } = amount;
    public int CreatedAt { get; set; } = createdAt;
    public string ManagerId { get; set; } = managerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public string CostCenter { get; set; } = costCenter;
    public string SalaryBand { get; set; } = salaryBand;
    public int GraceDays { get; set; } = graceDays;
    public string Notes { get; set; } = notes;
    public WeightedDeductionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a weighted deduction template in status created.</summary>
    public static WeightedDeductionTemplate NewWeightedDeductionTemplate(string code, string label)
    {
        return new WeightedDeductionTemplate(
            code,
            label,
            0,
            0.0,
            0,
            "",
            0,
            "",
            "",
            0,
            "",
            WeightedDeductionTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedDeductionTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedDeductionTemplateStatus next)
    {
        switch (Status)
        {
            case WeightedDeductionTemplateStatus.Created:
                return next == WeightedDeductionTemplateStatus.Validated;
            case WeightedDeductionTemplateStatus.Validated:
                switch (next)
                {
                    case WeightedDeductionTemplateStatus.Published:
                        return true;
                }
                return false;
            case WeightedDeductionTemplateStatus.Published:
                return next == WeightedDeductionTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedDeductionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted deduction template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"weighted deduction template"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)GraceDays - (double)Limit, 2);
    }

    public WeightedDeductionTemplate Copy()
    {
        return new WeightedDeductionTemplate(
            Code,
            Label,
            Limit,
            Amount,
            CreatedAt,
            ManagerId,
            ExpiresAt,
            CostCenter,
            SalaryBand,
            GraceDays,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
