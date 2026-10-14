namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted cost model group is in its life cycle.</summary>
public enum WeightedCostModelGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A weighted cost model group. Its code is unique within a tenant.</summary>
public sealed class WeightedCostModelGroup(string code, string title, string country, double discount, int priority, bool pinned, int quantity, Dictionary<string, string> attributes, string managerId, int retries, string location, WeightedCostModelGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public double Discount { get; set; } = discount;
    public int Priority { get; set; } = priority;
    public bool Pinned { get; set; } = pinned;
    public int Quantity { get; set; } = quantity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ManagerId { get; set; } = managerId;
    public int Retries { get; set; } = retries;
    public string Location { get; set; } = location;
    public WeightedCostModelGroupStatus Status { get; set; } = status;

    /// <summary>Starts a weighted cost model group in status idle.</summary>
    public static WeightedCostModelGroup NewWeightedCostModelGroup(string code, string title)
    {
        return new WeightedCostModelGroup(
            code,
            title,
            "",
            0.0,
            0,
            false,
            0,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            WeightedCostModelGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedCostModelGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedCostModelGroupStatus next)
    {
        switch (Status)
        {
            case WeightedCostModelGroupStatus.Idle:
                switch (next)
                {
                    case WeightedCostModelGroupStatus.Ready:
                        return true;
                }
                return false;
            case WeightedCostModelGroupStatus.Warming:
                switch (next)
                {
                    case WeightedCostModelGroupStatus.Draining:
                    case WeightedCostModelGroupStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedCostModelGroupStatus.Ready:
                return next == WeightedCostModelGroupStatus.Draining || next == WeightedCostModelGroupStatus.Stopped;
            case WeightedCostModelGroupStatus.Draining:
                return next == WeightedCostModelGroupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WeightedCostModelGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted cost model group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double NetTotal()
    {
        return Math.Round((double)Quantity - (double)Retries, 2);
    }

    public WeightedCostModelGroup Copy()
    {
        return new WeightedCostModelGroup(
            Code,
            Title,
            Country,
            Discount,
            Priority,
            Pinned,
            Quantity,
            Attributes,
            ManagerId,
            Retries,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
