namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary timesheet adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryTimesheetAdjustmentWorkflow(IPrimaryTimesheetAdjustmentStore Store, PrimaryTimesheetAdjustmentValidator Validator, IClock Clock, List<string> Journal, PrimaryTimesheetAdjustmentEngine Engine)
{
    public static PrimaryTimesheetAdjustmentWorkflow NewPrimaryTimesheetAdjustmentWorkflow(IPrimaryTimesheetAdjustmentStore store, IClock clock)
    {
        return new PrimaryTimesheetAdjustmentWorkflow(
            store,
            PrimaryTimesheetAdjustmentRules.DefaultPrimaryTimesheetAdjustmentValidator(),
            clock,
            new List<string>(),
            PrimaryTimesheetAdjustmentEngine.DefaultPrimaryTimesheetAdjustmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary timesheet adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryTimesheetAdjustmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary timesheet adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryTimesheetAdjustmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary timesheet adjustment {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}
