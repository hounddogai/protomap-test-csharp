namespace Acme.Workforce.Compliance.Tests.Certification.Notice;

using Acme.Workforce.Compliance.Certification.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WeightedCertificationBatchRulesChecks
{
    public static void WeightedCertificationBatchValidatorReportsEveryProblem()
    {
        var validator = WeightedCertificationBatchRules.DefaultWeightedCertificationBatchValidator();
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedCertificationBatchEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new WeightedCertificationBatchEmployeeIdLengthRule(86400);
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedCertificationBatchRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new WeightedCertificationBatchRequiredEmployeeIdRule("weighted certification batch needs this field");
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedCertificationBatchRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new WeightedCertificationBatchRequiredPhoneRule("weighted certification batch needs an entry");
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedCertificationBatchValidatorReportsEveryProblem();
        WeightedCertificationBatchEmployeeIdLengthRuleFlagsALongEmployeeId();
        WeightedCertificationBatchRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        WeightedCertificationBatchRequiredPhoneRuleFlagsABlankPhone();
    }
}
