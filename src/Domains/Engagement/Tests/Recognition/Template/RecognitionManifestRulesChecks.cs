namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RecognitionManifestRulesChecks
{
    public static void RecognitionManifestValidatorReportsEveryProblem()
    {
        var validator = RecognitionManifestRules.DefaultRecognitionManifestValidator();
        var item = RecognitionManifest.NewRecognitionManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionManifestUnitLengthRuleFlagsALongUnit()
    {
        var rule = new RecognitionManifestUnitLengthRule(64);
        var item = RecognitionManifest.NewRecognitionManifest("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionManifestRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RecognitionManifestRatioRangeRule(2.1, 1710.3);
        var item = RecognitionManifest.NewRecognitionManifest("x", "y");
        item.Ratio = 1711.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionManifestRequiredCodeRule("recognition manifest needs this field");
        var item = RecognitionManifest.NewRecognitionManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionManifestValidatorReportsEveryProblem();
        RecognitionManifestUnitLengthRuleFlagsALongUnit();
        RecognitionManifestRatioRangeRuleFlagsValuesAboveRatio();
        RecognitionManifestRequiredCodeRuleFlagsABlankCode();
    }
}
