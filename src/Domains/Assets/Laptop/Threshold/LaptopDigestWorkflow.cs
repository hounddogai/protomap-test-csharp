namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves laptop digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopDigestWorkflow(ILaptopDigestStore Store, LaptopDigestValidator Validator, IClock Clock, List<string> Journal)
{
    public static LaptopDigestWorkflow NewLaptopDigestWorkflow(ILaptopDigestStore store, IClock clock)
    {
        return new LaptopDigestWorkflow(
            store,
            LaptopDigestRules.DefaultLaptopDigestValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopDigestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopDigestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.UpdatedAt > cutoff)
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
