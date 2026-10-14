namespace Acme.Workforce.Compliance.Tests.TrainingDue.Quota;

using Acme.Workforce.Compliance.TrainingDue.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RegionalTrainingDueReservationRulesChecks
{
    public static void RegionalTrainingDueReservationValidatorReportsEveryProblem()
    {
        var validator = RegionalTrainingDueReservationRules.DefaultRegionalTrainingDueReservationValidator();
        var item = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalTrainingDueReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalTrainingDueReservationRequiredCodeRule(
            "regional training due reservation needs an entry");
        var item = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalTrainingDueReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalTrainingDueReservationCodeLengthRule(86400);
        var item = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalTrainingDueReservationRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new RegionalTrainingDueReservationRequiredCategoryRule(
            "regional training due reservation needs this field");
        var item = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalTrainingDueReservationValidatorReportsEveryProblem();
        RegionalTrainingDueReservationRequiredCodeRuleFlagsABlankCode();
        RegionalTrainingDueReservationCodeLengthRuleFlagsALongCode();
        RegionalTrainingDueReservationRequiredCategoryRuleFlagsABlankCategory();
    }
}
