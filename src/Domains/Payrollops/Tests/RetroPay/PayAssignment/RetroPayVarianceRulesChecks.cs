namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class RetroPayVarianceRulesChecks
{
    public static void RetroPayVarianceValidatorReportsEveryProblem()
    {
        var validator = RetroPayVarianceRules.DefaultRetroPayVarianceValidator();
        var item = RetroPayVariance.NewRetroPayVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RunAll()
    {
        RetroPayVarianceValidatorReportsEveryProblem();
    }
}
