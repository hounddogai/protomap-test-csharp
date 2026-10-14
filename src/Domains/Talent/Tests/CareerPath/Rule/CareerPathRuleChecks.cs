namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class CareerPathRuleChecks
{
    public static void CareerPathRuleStartsInCreated()
    {
        var item = CareerPathRule.NewCareerPathRule("code-145", "title-248");
        Check.Equal(item.Status, CareerPathRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathRuleMovesToPublished()
    {
        var item = CareerPathRule.NewCareerPathRule("code-145", "title-248");
        try
        {
            item.MoveTo(CareerPathRuleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathRuleStatus.Published, "status");
    }

    public static void CareerPathRuleRejectsMovesFromRetired()
    {
        var item = CareerPathRule.NewCareerPathRule("code-145", "title-248");
        item.Status = CareerPathRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(CareerPathRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        CareerPathRuleStartsInCreated();
        CareerPathRuleMovesToPublished();
        CareerPathRuleRejectsMovesFromRetired();
    }
}
