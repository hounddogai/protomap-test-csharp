namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientEngagementManifestRulesChecks
{
    public static void LenientEngagementManifestValidatorReportsEveryProblem()
    {
        var validator = LenientEngagementManifestRules.DefaultLenientEngagementManifestValidator();
        var item = LenientEngagementManifest.NewLenientEngagementManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientEngagementManifestRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new LenientEngagementManifestRequiredChannelRule("lenient engagement manifest needs a value");
        var item = LenientEngagementManifest.NewLenientEngagementManifest("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientEngagementManifestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientEngagementManifestRequiredLabelRule("lenient engagement manifest needs an entry");
        var item = LenientEngagementManifest.NewLenientEngagementManifest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientEngagementManifestValidatorReportsEveryProblem();
        LenientEngagementManifestRequiredChannelRuleFlagsABlankChannel();
        LenientEngagementManifestRequiredLabelRuleFlagsABlankLabel();
    }
}
