namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountWindowManager(IHeadcountWindowStore Store, HeadcountWindowValidator Validator, IClock Clock, List<string> Journal, HeadcountWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountWindowStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountWindowStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount window {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of headcount window workflow.</summary>
public static class HeadcountWindowWorkflow
{
    public static HeadcountWindowManager NewHeadcountWindowManager(IHeadcountWindowStore store, IClock clock)
    {
        return new HeadcountWindowManager(
            store,
            HeadcountWindowRules.DefaultHeadcountWindowValidator(),
            clock,
            new List<string>(),
            HeadcountWindowEngine.DefaultHeadcountWindowEngine());
    }
}
