namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default laptop segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultLaptopSegmentWorkflow(IDefaultLaptopSegmentStore Store, DefaultLaptopSegmentValidator Validator, IClock Clock, List<string> Journal, DefaultLaptopSegmentEngine Engine)
{
    public static DefaultLaptopSegmentWorkflow NewDefaultLaptopSegmentWorkflow(IDefaultLaptopSegmentStore store, IClock clock, Registry flags)
    {
        return new DefaultLaptopSegmentWorkflow(
            store,
            DefaultLaptopSegmentRules.DefaultDefaultLaptopSegmentValidator(),
            clock,
            new List<string>(),
            DefaultLaptopSegmentEngine.DefaultDefaultLaptopSegmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default laptop segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultLaptopSegmentStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default laptop segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultLaptopSegmentStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default laptop segment {code} not found");
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
