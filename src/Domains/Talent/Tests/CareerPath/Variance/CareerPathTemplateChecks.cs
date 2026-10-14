namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class CareerPathTemplateChecks
{
    public static void CareerPathTemplateStartsInRequested()
    {
        var item = CareerPathTemplate.NewCareerPathTemplate("code-571", "name-880");
        Check.Equal(item.Status, CareerPathTemplateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathTemplateMovesToExpired()
    {
        var item = CareerPathTemplate.NewCareerPathTemplate("code-571", "name-880");
        try
        {
            item.MoveTo(CareerPathTemplateStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathTemplateStatus.Expired, "status");
    }

    public static void CareerPathTemplateRejectsMovesFromExpired()
    {
        var item = CareerPathTemplate.NewCareerPathTemplate("code-571", "name-880");
        item.Status = CareerPathTemplateStatus.Expired;
        Check.Fails(() => item.MoveTo(CareerPathTemplateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CareerPathTemplateStartsInRequested();
        CareerPathTemplateMovesToExpired();
        CareerPathTemplateRejectsMovesFromExpired();
    }
}
