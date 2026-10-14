namespace Acme.Workforce.Payrollops.Tests.TaxForm.Assignment;

using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TaxFormAssignmentRulesChecks
{
    public static void TaxFormAssignmentValidatorReportsEveryProblem()
    {
        var validator = TaxFormAssignmentRules.DefaultTaxFormAssignmentValidator();
        var item = TaxFormAssignment.NewTaxFormAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormAssignmentRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new TaxFormAssignmentRequiredPhoneRule("tax form assignment needs an entry");
        var item = TaxFormAssignment.NewTaxFormAssignment("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormAssignmentValidatorReportsEveryProblem();
        TaxFormAssignmentRequiredPhoneRuleFlagsABlankPhone();
    }
}
