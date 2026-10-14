namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model bundle is in its life cycle.</summary>
public enum CostModelBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a cost model bundle and its current status.</summary>
public sealed class CostModelBundle(string code, string label, Dictionary<string, string> attributes, double rate, int capacity, CostModelBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public int Capacity { get; set; } = capacity;
    public CostModelBundleStatus Status { get; set; } = status;

    /// <summary>Starts a cost model bundle in status proposed.</summary>
    public static CostModelBundle NewCostModelBundle(string code, string label)
    {
        return new CostModelBundle(
            code,
            label,
            new Dictionary<string, string>(),
            0.0,
            0,
            CostModelBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelBundleStatus next)
    {
        switch (Status)
        {
            case CostModelBundleStatus.Proposed:
                return next == CostModelBundleStatus.Rejected || next == CostModelBundleStatus.Withdrawn;
            case CostModelBundleStatus.Approved:
                return next == CostModelBundleStatus.Withdrawn;
            case CostModelBundleStatus.Rejected:
                return next == CostModelBundleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CostModelBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double WeightedExposure()
    {
        return Math.Round((double)Capacity + Rate, 2);
    }

    public CostModelBundle Copy()
    {
        return new CostModelBundle(Code, Label, Attributes, Rate, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
