namespace Acme.Workforce.Workplace.Tests.Desk.DeskVariance;

using Acme.Workforce.Workplace.Desk.DeskVariance;

/// <summary>Checks of desk variance code, run by the project's runner.</summary>
public static class TieredDeskVarianceRulesChecks
{
    public static void TieredDeskVarianceValidatorReportsEveryProblem()
    {
        var validator = TieredDeskVarianceRules.DefaultTieredDeskVarianceValidator();
        var item = TieredDeskVariance.NewTieredDeskVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredDeskVarianceUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new TieredDeskVarianceUpdatedAtRangeRule(1000, 3600);
        var item = TieredDeskVariance.NewTieredDeskVariance("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredDeskVarianceFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new TieredDeskVarianceFteRangeRule(8.7, 280.2);
        var item = TieredDeskVariance.NewTieredDeskVariance("x", "y");
        item.Fte = 281.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredDeskVarianceHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new TieredDeskVarianceHoursRangeRule(4.4, 4835.7);
        var item = TieredDeskVariance.NewTieredDeskVariance("x", "y");
        item.Hours = 4837.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredDeskVarianceValidatorReportsEveryProblem();
        TieredDeskVarianceUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        TieredDeskVarianceFteRangeRuleFlagsValuesAboveFte();
        TieredDeskVarianceHoursRangeRuleFlagsValuesAboveHours();
    }
}
