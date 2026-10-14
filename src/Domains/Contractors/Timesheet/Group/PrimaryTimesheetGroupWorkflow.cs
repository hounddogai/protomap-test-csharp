namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary timesheet groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryTimesheetGroupManager(IPrimaryTimesheetGroupStore Store, PrimaryTimesheetGroupValidator Validator, IClock Clock, List<string> Journal, PrimaryTimesheetGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary timesheet group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryTimesheetGroupStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary timesheet group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryTimesheetGroupStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary timesheet group {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of primary timesheet group workflow.</summary>
public static class PrimaryTimesheetGroupWorkflow
{
    public static PrimaryTimesheetGroupManager NewPrimaryTimesheetGroupManager(IPrimaryTimesheetGroupStore store, IClock clock, Registry flags)
    {
        return new PrimaryTimesheetGroupManager(
            store,
            PrimaryTimesheetGroupRules.DefaultPrimaryTimesheetGroupValidator(),
            clock,
            new List<string>(),
            PrimaryTimesheetGroupEngine.DefaultPrimaryTimesheetGroupEngine(flags));
    }
}
