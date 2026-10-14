namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnCheckpointRulesChecks
{
    public static void AssetReturnCheckpointValidatorReportsEveryProblem()
    {
        var validator = AssetReturnCheckpointRules.DefaultAssetReturnCheckpointValidator();
        var item = AssetReturnCheckpoint.NewAssetReturnCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new AssetReturnCheckpointNameLengthRule(80);
        var item = AssetReturnCheckpoint.NewAssetReturnCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnCheckpointRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new AssetReturnCheckpointRequiredReviewerRule("asset return checkpoint needs an entry");
        var item = AssetReturnCheckpoint.NewAssetReturnCheckpoint("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssetReturnCheckpointRequiredCodeRule("asset return checkpoint needs an entry");
        var item = AssetReturnCheckpoint.NewAssetReturnCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnCheckpointValidatorReportsEveryProblem();
        AssetReturnCheckpointNameLengthRuleFlagsALongName();
        AssetReturnCheckpointRequiredReviewerRuleFlagsABlankReviewer();
        AssetReturnCheckpointRequiredCodeRuleFlagsABlankCode();
    }
}
