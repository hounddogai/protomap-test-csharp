namespace Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>A rule a payslip reservation must pass. Check returns its problems, or none.</summary>
public interface IPayslipReservationRule
{
    List<string> Check(PayslipReservation item);
}

public sealed record PayslipReservationRequiredNameRule(string Message) : IPayslipReservationRule
{
    public List<string> Check(PayslipReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipReservationCodeLengthRule(int MaxLength) : IPayslipReservationRule
{
    public List<string> Check(PayslipReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipReservationRequiredCostCenterRule(string Message) : IPayslipReservationRule
{
    public List<string> Check(PayslipReservation item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip reservation.</summary>
public sealed record PayslipReservationValidator(List<IPayslipReservationRule> Rules)
{
    public List<string> Validate(PayslipReservation item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(PayslipReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipReservation> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of payslip reservation rules.</summary>
public static class PayslipReservationRules
{
    public static PayslipReservationValidator DefaultPayslipReservationValidator()
    {
        return new PayslipReservationValidator(
            new List<IPayslipReservationRule>
            {
                new PayslipReservationRequiredNameRule("payslip reservation needs a value"),
                new PayslipReservationCodeLengthRule(255),
                new PayslipReservationRequiredCostCenterRule("payslip reservation needs a value"),
            });
    }
}
