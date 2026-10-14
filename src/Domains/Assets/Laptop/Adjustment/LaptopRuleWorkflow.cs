namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves laptop rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopRuleDesk(ILaptopRuleStore Store, LaptopRuleValidator Validator, IClock Clock, List<string> Journal, LaptopRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopRuleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopRuleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop rule {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of laptop rule workflow.</summary>
public static class LaptopRuleWorkflow
{
    public static LaptopRuleDesk NewLaptopRuleDesk(ILaptopRuleStore store, IClock clock)
    {
        return new LaptopRuleDesk(
            store,
            LaptopRuleRules.DefaultLaptopRuleValidator(),
            clock,
            new List<string>(),
            LaptopRuleEngine.DefaultLaptopRuleEngine());
    }
}
