namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool override is in its life cycle.</summary>
public enum PoolOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a pool override and its current status.</summary>
public sealed class PoolOverride(string code, string name, string externalRef, Dictionary<string, string> attributes, string ownerId, int batchSize, bool pinned, int sequence, string title, double amount, PoolOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string OwnerId { get; set; } = ownerId;
    public int BatchSize { get; set; } = batchSize;
    public bool Pinned { get; set; } = pinned;
    public int Sequence { get; set; } = sequence;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public PoolOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a pool override in status new.</summary>
    public static PoolOverride NewPoolOverride(string code, string name)
    {
        return new PoolOverride(
            code,
            name,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            false,
            0,
            "",
            0.0,
            PoolOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolOverrideStatus next)
    {
        switch (Status)
        {
            case PoolOverrideStatus.New:
                switch (next)
                {
                    case PoolOverrideStatus.Running:
                        return true;
                }
                return false;
            case PoolOverrideStatus.Queued:
                switch (next)
                {
                    case PoolOverrideStatus.Done:
                        return true;
                }
                return false;
            case PoolOverrideStatus.Running:
                switch (next)
                {
                    case PoolOverrideStatus.Done:
                    case PoolOverrideStatus.Failed:
                        return true;
                }
                return false;
            case PoolOverrideStatus.Done:
                switch (next)
                {
                    case PoolOverrideStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetVolume()
    {
        return Math.Round((double)Sequence - Amount, 2);
    }

    public PoolOverride Copy()
    {
        return new PoolOverride(
            Code,
            Name,
            ExternalRef,
            Attributes,
            OwnerId,
            BatchSize,
            Pinned,
            Sequence,
            Title,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
