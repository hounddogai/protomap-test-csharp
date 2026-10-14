namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PrimaryTimesheetGroupEngineChecks
{
    public static void PrimaryTimesheetGroupEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryTimesheetGroupEngine.DefaultPrimaryTimesheetGroupEngine(Flags.NewRegistry());
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("code-823", "name-552");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryTimesheetGroupEngineKeepsResultsWithinBounds();
    }
}
