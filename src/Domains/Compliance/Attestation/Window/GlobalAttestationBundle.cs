namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global attestation bundle is in its life cycle.</summary>
public enum GlobalAttestationBundleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A global attestation bundle. Its code is unique within a tenant.</summary>
public sealed class GlobalAttestationBundle(string code, string name, bool pinned, string label, int batchSize, int capacity, double ceiling, GlobalAttestationBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public int Capacity { get; set; } = capacity;
    public double Ceiling { get; set; } = ceiling;
    public GlobalAttestationBundleStatus Status { get; set; } = status;

    /// <summary>Starts a global attestation bundle in status created.</summary>
    public static GlobalAttestationBundle NewGlobalAttestationBundle(string code, string name)
    {
        return new GlobalAttestationBundle(code, name, false, "", 0, 0, 0.0, GlobalAttestationBundleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAttestationBundleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAttestationBundleStatus next)
    {
        switch (Status)
        {
            case GlobalAttestationBundleStatus.Created:
                return next == GlobalAttestationBundleStatus.Validated || next == GlobalAttestationBundleStatus.Retired;
            case GlobalAttestationBundleStatus.Validated:
                switch (next)
                {
                    case GlobalAttestationBundleStatus.Published:
                    case GlobalAttestationBundleStatus.Retired:
                        return true;
                }
                return false;
            case GlobalAttestationBundleStatus.Published:
                return next == GlobalAttestationBundleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GlobalAttestationBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global attestation bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Capacity * Ceiling, 2);
    }

    public GlobalAttestationBundle Copy()
    {
        return new GlobalAttestationBundle(Code, Name, Pinned, Label, BatchSize, Capacity, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
