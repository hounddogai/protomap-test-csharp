namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class ManualTaxFormMetricChecks
{
    public static void ManualTaxFormMetricStartsInOpen()
    {
        var item = ManualTaxFormMetric.NewManualTaxFormMetric("code-797", "title-380");
        Check.Equal(item.Status, ManualTaxFormMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualTaxFormMetricMovesToArchived()
    {
        var item = ManualTaxFormMetric.NewManualTaxFormMetric("code-797", "title-380");
        try
        {
            item.MoveTo(ManualTaxFormMetricStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualTaxFormMetricStatus.Archived, "status");
    }

    public static void ManualTaxFormMetricRejectsMovesFromArchived()
    {
        var item = ManualTaxFormMetric.NewManualTaxFormMetric("code-797", "title-380");
        item.Status = ManualTaxFormMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(ManualTaxFormMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        ManualTaxFormMetricStartsInOpen();
        ManualTaxFormMetricMovesToArchived();
        ManualTaxFormMetricRejectsMovesFromArchived();
    }
}
