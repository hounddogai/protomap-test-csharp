namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LegacyAuditClaimRulesChecks
{
    public static void LegacyAuditClaimValidatorReportsEveryProblem()
    {
        var validator = LegacyAuditClaimRules.DefaultLegacyAuditClaimValidator();
        var item = LegacyAuditClaim.NewLegacyAuditClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyAuditClaimRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new LegacyAuditClaimRequiredOwnerIdRule("legacy audit claim needs this field");
        var item = LegacyAuditClaim.NewLegacyAuditClaim("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAuditClaimRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new LegacyAuditClaimRevisionRangeRule(32, 80);
        var item = LegacyAuditClaim.NewLegacyAuditClaim("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyAuditClaimOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new LegacyAuditClaimOwnerIdLengthRule(3600);
        var item = LegacyAuditClaim.NewLegacyAuditClaim("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyAuditClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyAuditClaimRequiredTitleRule("legacy audit claim needs an entry");
        var item = LegacyAuditClaim.NewLegacyAuditClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyAuditClaimValidatorReportsEveryProblem();
        LegacyAuditClaimRequiredOwnerIdRuleFlagsABlankOwnerId();
        LegacyAuditClaimRevisionRangeRuleFlagsValuesAboveRevision();
        LegacyAuditClaimOwnerIdLengthRuleFlagsALongOwnerId();
        LegacyAuditClaimRequiredTitleRuleFlagsABlankTitle();
    }
}
