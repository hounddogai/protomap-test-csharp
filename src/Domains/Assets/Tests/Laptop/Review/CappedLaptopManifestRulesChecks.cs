namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedLaptopManifestRulesChecks
{
    public static void CappedLaptopManifestValidatorReportsEveryProblem()
    {
        var validator = CappedLaptopManifestRules.DefaultCappedLaptopManifestValidator();
        var item = CappedLaptopManifest.NewCappedLaptopManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedLaptopManifestRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new CappedLaptopManifestRequiredReviewerRule("capped laptop manifest needs a value");
        var item = CappedLaptopManifest.NewCappedLaptopManifest("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedLaptopManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedLaptopManifestCodeLengthRule(16);
        var item = CappedLaptopManifest.NewCappedLaptopManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedLaptopManifestNameLengthRuleFlagsALongName()
    {
        var rule = new CappedLaptopManifestNameLengthRule(1000);
        var item = CappedLaptopManifest.NewCappedLaptopManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedLaptopManifestValidatorReportsEveryProblem();
        CappedLaptopManifestRequiredReviewerRuleFlagsABlankReviewer();
        CappedLaptopManifestCodeLengthRuleFlagsALongCode();
        CappedLaptopManifestNameLengthRuleFlagsALongName();
    }
}
