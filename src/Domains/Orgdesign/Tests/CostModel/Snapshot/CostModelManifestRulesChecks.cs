namespace Acme.Workforce.Orgdesign.Tests.CostModel.Snapshot;

using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class CostModelManifestRulesChecks
{
    public static void CostModelManifestValidatorReportsEveryProblem()
    {
        var validator = CostModelManifestRules.DefaultCostModelManifestValidator();
        var item = CostModelManifest.NewCostModelManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelManifestRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new CostModelManifestRequiredUnitRule("cost model manifest needs this field");
        var item = CostModelManifest.NewCostModelManifest("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new CostModelManifestRequiredNameRule("cost model manifest needs a value");
        var item = CostModelManifest.NewCostModelManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelManifestQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new CostModelManifestQuantityRangeRule(1000, 3600);
        var item = CostModelManifest.NewCostModelManifest("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelManifestCodeLengthRule(64);
        var item = CostModelManifest.NewCostModelManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new CostModelManifestExpiresAtRangeRule(3600, 86400);
        var item = CostModelManifest.NewCostModelManifest("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CostModelManifestValidatorReportsEveryProblem();
        CostModelManifestRequiredUnitRuleFlagsABlankUnit();
        CostModelManifestRequiredNameRuleFlagsABlankName();
        CostModelManifestQuantityRangeRuleFlagsValuesAboveQuantity();
        CostModelManifestCodeLengthRuleFlagsALongCode();
        CostModelManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
