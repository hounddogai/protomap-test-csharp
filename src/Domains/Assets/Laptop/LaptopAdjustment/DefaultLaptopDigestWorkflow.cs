namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default laptop digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultLaptopDigestDesk(IDefaultLaptopDigestStore Store, DefaultLaptopDigestValidator Validator, IClock Clock, List<string> Journal, DefaultLaptopDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default laptop digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultLaptopDigestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default laptop digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultLaptopDigestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default laptop digest {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of default laptop digest workflow.</summary>
public static class DefaultLaptopDigestWorkflow
{
    public static DefaultLaptopDigestDesk NewDefaultLaptopDigestDesk(IDefaultLaptopDigestStore store, IClock clock, Registry flags)
    {
        return new DefaultLaptopDigestDesk(
            store,
            DefaultLaptopDigestRules.DefaultDefaultLaptopDigestValidator(),
            clock,
            new List<string>(),
            DefaultLaptopDigestEngine.DefaultDefaultLaptopDigestEngine(flags));
    }
}
