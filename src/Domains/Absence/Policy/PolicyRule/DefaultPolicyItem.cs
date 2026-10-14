namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default policy item is in its life cycle.</summary>
public enum DefaultPolicyItemStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One default policy item, with what the planners need to act on it.</summary>
public sealed class DefaultPolicyItem(string code, string name, string notes, int retries, int capacity, string externalRef, double amount, int revision, int graceDays, bool approved, DefaultPolicyItemStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public int Retries { get; set; } = retries;
    public int Capacity { get; set; } = capacity;
    public string ExternalRef { get; set; } = externalRef;
    public double Amount { get; set; } = amount;
    public int Revision { get; set; } = revision;
    public int GraceDays { get; set; } = graceDays;
    public bool Approved { get; set; } = approved;
    public DefaultPolicyItemStatus Status { get; set; } = status;

    /// <summary>Starts a default policy item in status created.</summary>
    public static DefaultPolicyItem NewDefaultPolicyItem(string code, string name)
    {
        return new DefaultPolicyItem(code, name, "", 0, 0, "", 0.0, 0, 0, false, DefaultPolicyItemStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPolicyItemStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPolicyItemStatus next)
    {
        switch (Status)
        {
            case DefaultPolicyItemStatus.Created:
                switch (next)
                {
                    case DefaultPolicyItemStatus.Retired:
                        return true;
                }
                return false;
            case DefaultPolicyItemStatus.Validated:
                return next == DefaultPolicyItemStatus.Published;
            case DefaultPolicyItemStatus.Published:
                switch (next)
                {
                    case DefaultPolicyItemStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPolicyItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default policy item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetLoad()
    {
        return Math.Round((double)Capacity + (double)Retries, 2);
    }

    public DefaultPolicyItem Copy()
    {
        return new DefaultPolicyItem(
            Code,
            Name,
            Notes,
            Retries,
            Capacity,
            ExternalRef,
            Amount,
            Revision,
            GraceDays,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
