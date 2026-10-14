namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped payslip reservation must pass. Check returns its problems, or none.</summary>
public interface ICappedPayslipReservationRule
{
    List<string> Check(CappedPayslipReservation item);
}

public sealed record CappedPayslipReservationAllowedLabelRule(List<string> Allowed) : ICappedPayslipReservationRule
{
    public List<string> Check(CappedPayslipReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is too long" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipReservationLabelLengthRule(int MaxLength) : ICappedPayslipReservationRule
{
    public List<string> Check(CappedPayslipReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipReservationRequiredLabelRule(string Message) : ICappedPayslipReservationRule
{
    public List<string> Check(CappedPayslipReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipReservationCodeLengthRule(int MaxLength) : ICappedPayslipReservationRule
{
    public List<string> Check(CappedPayslipReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped payslip reservation.</summary>
public sealed record CappedPayslipReservationValidator(List<ICappedPayslipReservationRule> Rules)
{
    public List<string> Validate(CappedPayslipReservation item)
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

    public void RequireValid(CappedPayslipReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayslipReservation> items)
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

/// <summary>Functions of capped payslip reservation rules.</summary>
public static class CappedPayslipReservationRules
{
    public static CappedPayslipReservationValidator DefaultCappedPayslipReservationValidator()
    {
        return new CappedPayslipReservationValidator(
            new List<ICappedPayslipReservationRule>
            {
                new CappedPayslipReservationAllowedLabelRule(new List<string>()),
                new CappedPayslipReservationLabelLengthRule(16),
                new CappedPayslipReservationRequiredLabelRule("capped payslip reservation needs an entry"),
                new CappedPayslipReservationCodeLengthRule(86400),
            });
    }
}
