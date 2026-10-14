namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountMappingWorkflow(IHeadcountMappingStore Store, HeadcountMappingValidator Validator, IClock Clock, List<string> Journal, HeadcountMappingEngine Engine)
{
    public static HeadcountMappingWorkflow NewHeadcountMappingWorkflow(IHeadcountMappingStore store, IClock clock, Registry flags)
    {
        return new HeadcountMappingWorkflow(
            store,
            HeadcountMappingRules.DefaultHeadcountMappingValidator(),
            clock,
            new List<string>(),
            HeadcountMappingEngine.DefaultHeadcountMappingEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountMappingStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountMappingStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount mapping {code} not found");
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
