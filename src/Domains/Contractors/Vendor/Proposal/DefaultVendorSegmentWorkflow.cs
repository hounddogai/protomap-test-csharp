namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default vendor segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultVendorSegmentWorkflow(IDefaultVendorSegmentStore Store, DefaultVendorSegmentValidator Validator, IClock Clock, List<string> Journal, DefaultVendorSegmentEngine Engine)
{
    public static DefaultVendorSegmentWorkflow NewDefaultVendorSegmentWorkflow(IDefaultVendorSegmentStore store, IClock clock, Registry flags)
    {
        return new DefaultVendorSegmentWorkflow(
            store,
            DefaultVendorSegmentRules.DefaultDefaultVendorSegmentValidator(),
            clock,
            new List<string>(),
            DefaultVendorSegmentEngine.DefaultDefaultVendorSegmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default vendor segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultVendorSegmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default vendor segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultVendorSegmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default vendor segment {code} not found");
        }
        var result = Engine.Run(item);
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
