namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary succession plan records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimarySuccessionPlanRecordService(IPrimarySuccessionPlanRecordStore Store, PrimarySuccessionPlanRecordValidator Validator, IClock Clock, List<string> Journal, PrimarySuccessionPlanRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary succession plan record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimarySuccessionPlanRecordStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary succession plan record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimarySuccessionPlanRecordStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary succession plan record {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of primary succession plan record workflow.</summary>
public static class PrimarySuccessionPlanRecordWorkflow
{
    public static PrimarySuccessionPlanRecordService NewPrimarySuccessionPlanRecordService(IPrimarySuccessionPlanRecordStore store, IClock clock, Registry flags)
    {
        return new PrimarySuccessionPlanRecordService(
            store,
            PrimarySuccessionPlanRecordRules.DefaultPrimarySuccessionPlanRecordValidator(),
            clock,
            new List<string>(),
            PrimarySuccessionPlanRecordEngine.DefaultPrimarySuccessionPlanRecordEngine(flags));
    }
}
