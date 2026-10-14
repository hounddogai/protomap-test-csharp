namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary timesheet exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryTimesheetExceptionService(IPrimaryTimesheetExceptionStore Store, PrimaryTimesheetExceptionValidator Validator, IClock Clock, List<string> Journal, PrimaryTimesheetExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary timesheet exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryTimesheetExceptionStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary timesheet exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryTimesheetExceptionStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary timesheet exception {code} not found");
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

/// <summary>Functions of primary timesheet exception workflow.</summary>
public static class PrimaryTimesheetExceptionWorkflow
{
    public static PrimaryTimesheetExceptionService NewPrimaryTimesheetExceptionService(IPrimaryTimesheetExceptionStore store, IClock clock, Registry flags)
    {
        return new PrimaryTimesheetExceptionService(
            store,
            PrimaryTimesheetExceptionRules.DefaultPrimaryTimesheetExceptionValidator(),
            clock,
            new List<string>(),
            PrimaryTimesheetExceptionEngine.DefaultPrimaryTimesheetExceptionEngine(flags));
    }
}
