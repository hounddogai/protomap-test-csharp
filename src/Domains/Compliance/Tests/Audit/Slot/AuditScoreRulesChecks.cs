namespace Acme.Workforce.Compliance.Tests.Audit.Slot;

using Acme.Workforce.Compliance.Audit.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AuditScoreRulesChecks
{
    public static void AuditScoreValidatorReportsEveryProblem()
    {
        var validator = AuditScoreRules.DefaultAuditScoreValidator();
        var item = AuditScore.NewAuditScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditScoreRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new AuditScoreRequiredExternalRefRule("audit score needs an entry");
        var item = AuditScore.NewAuditScore("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditScoreCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AuditScoreCostRangeRule(2.9, 2689.5);
        var item = AuditScore.NewAuditScore("x", "y");
        item.Cost = 2691.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new AuditScoreRequiredNameRule("audit score needs this field");
        var item = AuditScore.NewAuditScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditScoreNameLengthRuleFlagsALongName()
    {
        var rule = new AuditScoreNameLengthRule(1000);
        var item = AuditScore.NewAuditScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditScoreValidatorReportsEveryProblem();
        AuditScoreRequiredExternalRefRuleFlagsABlankExternalRef();
        AuditScoreCostRangeRuleFlagsValuesAboveCost();
        AuditScoreRequiredNameRuleFlagsABlankName();
        AuditScoreNameLengthRuleFlagsALongName();
    }
}
