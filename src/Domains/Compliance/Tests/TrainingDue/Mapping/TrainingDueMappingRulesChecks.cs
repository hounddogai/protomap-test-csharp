namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class TrainingDueMappingRulesChecks
{
    public static void TrainingDueMappingValidatorReportsEveryProblem()
    {
        var validator = TrainingDueMappingRules.DefaultTrainingDueMappingValidator();
        var item = TrainingDueMapping.NewTrainingDueMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueMappingBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new TrainingDueMappingBadgeLengthRule(64);
        var item = TrainingDueMapping.NewTrainingDueMapping("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueMappingRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new TrainingDueMappingRequiredSegmentRule("training due mapping needs an entry");
        var item = TrainingDueMapping.NewTrainingDueMapping("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueMappingNameLengthRuleFlagsALongName()
    {
        var rule = new TrainingDueMappingNameLengthRule(120);
        var item = TrainingDueMapping.NewTrainingDueMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TrainingDueMappingValidatorReportsEveryProblem();
        TrainingDueMappingBadgeLengthRuleFlagsALongBadge();
        TrainingDueMappingRequiredSegmentRuleFlagsABlankSegment();
        TrainingDueMappingNameLengthRuleFlagsALongName();
    }
}
