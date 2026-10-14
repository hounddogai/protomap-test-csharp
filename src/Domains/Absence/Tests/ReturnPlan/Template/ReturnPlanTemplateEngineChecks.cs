namespace Acme.Workforce.Absence.Tests.ReturnPlan.Template;

using Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ReturnPlanTemplateEngineChecks
{
    public static void ReturnPlanTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanTemplateEngine.DefaultReturnPlanTemplateEngine();
        var item = ReturnPlanTemplate.NewReturnPlanTemplate("code-731", "label-35");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanTemplateEngineKeepsResultsWithinBounds();
    }
}
